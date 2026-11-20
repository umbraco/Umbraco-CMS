// PostToolUse hook: lint backoffice client files right after Claude edits them,
// so lint errors are fixed in the same turn instead of in review.
const { spawnSync } = require('child_process');
const fs = require('fs');
const path = require('path');

let raw = '';
process.stdin.on('data', (chunk) => (raw += chunk)).on('end', () => {
	const file = (JSON.parse(raw).tool_input || {}).file_path || '';
	const projectDir = process.env.CLAUDE_PROJECT_DIR || process.cwd();
	const client = path.join(projectDir, 'src', 'Umbraco.Web.UI.Client');
	const rel = path.relative(client, file);

	if (rel.startsWith('..') || !rel.startsWith('src' + path.sep) || !/\.(ts|js)$/.test(rel)) return;

	// No local install (fresh worktree, CI): skip rather than let npx fetch an unpinned ESLint.
	const eslint = path.join(client, 'node_modules', 'eslint', 'bin', 'eslint.js');
	if (!fs.existsSync(eslint)) return;

	// Same bar as CI (`npm run lint:errors`): errors only, and ignored files are not a failure.
	const result = spawnSync(process.execPath, [eslint, '--quiet', '--no-warn-ignored', rel], {
		cwd: client,
		encoding: 'utf8',
	});

	if (result.status === 1) {
		console.error(`ESLint reported errors in ${rel}. Fix them:\n${result.stdout.trim()}`);
		process.exit(2);
	}

	// ESLint itself failed (config error, crash): report it without blocking the edit.
	if (result.status !== 0) {
		console.error(`ESLint hook skipped: ESLint could not lint ${rel}.\n${(result.stderr || '').trim()}`);
	}
});
