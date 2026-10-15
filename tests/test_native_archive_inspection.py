"""Inspection retains every member while avoiding per-member tool dispatch."""
import hashlib
import importlib.util
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest import mock

SCRIPT = Path(__file__).resolve().parents[1] / 'scripts/inspect-apple-archive.py'
spec = importlib.util.spec_from_file_location('archive_inspection', SCRIPT)
inspector = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inspector)


def archive_member(name, body):
    fields = [name.encode() + b'/', b'0', b'0', b'0', b'100644', str(len(body)).encode()]
    return b''.join(value.ljust(size, b' ') for value, size in zip(fields, (16,12,6,6,8,10))) + b'`\n' + body + (b'\n' if len(body) % 2 else b'')


class ArchiveInspectionTests(unittest.TestCase):
    def setUp(self):
        inspector.inspection_tool.cache_clear()
        self.addCleanup(inspector.inspection_tool.cache_clear)

    def test_resolves_once_per_tool_and_keeps_selected_xcode_path(self):
        with mock.patch.object(inspector, 'command', side_effect=lambda *args: '/selected Xcode/bin/' + args[-1]) as command:
            for _ in range(4):
                self.assertEqual(inspector.inspection_tool('vtool'), '/selected Xcode/bin/vtool')
                self.assertEqual(inspector.inspection_tool('lipo'), '/selected Xcode/bin/lipo')
            self.assertEqual(command.call_args_list, [mock.call('/usr/bin/xcrun', '--find', 'vtool'), mock.call('/usr/bin/xcrun', '--find', 'lipo')])
            with self.assertRaises(ValueError):
                inspector.inspection_tool('clang')

    def test_lookup_failure_and_invalid_path_fail_closed(self):
        with mock.patch.object(inspector, 'command', side_effect=subprocess.CalledProcessError(1, ['xcrun'])):
            with self.assertRaises(subprocess.CalledProcessError):
                inspector.inspection_tool('vtool')
        for path in ('', 'relative/tool'):
            with mock.patch.object(inspector, 'command', return_value=path), self.assertRaises(ValueError):
                inspector.inspection_tool('vtool')

    def test_every_member_and_duplicate_name_keeps_exact_metadata(self):
        payloads = [b'\xcf\xfa\xed\xfeowned-one', b'\xcf\xfa\xed\xfeowned-two']
        calls = []
        def run(*args):
            calls.append(args)
            if args[0] == '/usr/bin/xcrun':
                return '/selected/bin/' + args[-1]
            if args[0] == '/selected/bin/vtool':
                return 'platform IOS\nminos 15.0\nsdk 26.5'
            if args[0] == '/selected/bin/lipo':
                return 'arm64'
            self.fail('Unexpected command: ' + repr(args))
        with tempfile.TemporaryDirectory() as temporary, mock.patch.object(inspector, 'command', side_effect=run):
            root = Path(temporary)
            archive = root / 'owned.a'
            archive.write_bytes(b'!<arch>\n' + archive_member('__.SYMDEF', b'index') + b''.join(archive_member('same.o', body) for body in payloads) + archive_member('notes', b'other'))
            inspector.inspection_tool('vtool')
            inspector.inspection_tool('lipo')
            records = inspector.inspect_thin_archive(archive, 'arm64', root)
            expected = [{'index':1,'name':'__.SYMDEF','sha256':hashlib.sha256(b'index').hexdigest(),'size':5,'type':'archive-metadata'}]
            for index, body in enumerate(payloads, 2):
                expected.append({'index':index,'name':'same.o','sha256':hashlib.sha256(body).hexdigest(),'size':len(body),'type':'mach-o','architectures':['arm64'],'platform':'IOS','minos':'15.0','sdk':'26.5'})
            expected.append({'index':4,'name':'notes','sha256':hashlib.sha256(b'other').hexdigest(),'size':5,'type':'non-mach-o'})
            self.assertEqual(records, expected)
            self.assertEqual(sum(c[0]=='/selected/bin/vtool' for c in calls), 2)
            self.assertEqual(sum(c[0]=='/selected/bin/lipo' for c in calls), 2)
            self.assertEqual(sum(c[0]=='/usr/bin/xcrun' for c in calls), 2)

    def test_member_tool_failure_is_not_accepted(self):
        with tempfile.TemporaryDirectory() as temporary, mock.patch.object(inspector, 'inspection_tool', return_value='/selected/bin/vtool'), mock.patch.object(inspector, 'command', side_effect=subprocess.CalledProcessError(23, ['vtool'])):
            with self.assertRaises(subprocess.CalledProcessError):
                inspector.inspect_member((1, 'owned.o', b'\xcf\xfa\xed\xfeowned'), 'arm64', Path(temporary))


if __name__ == '__main__':
    unittest.main()
