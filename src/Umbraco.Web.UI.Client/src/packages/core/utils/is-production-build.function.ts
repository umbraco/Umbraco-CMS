/**
 * Returns true if the current build is a production build, false otherwise.
 *
 * Vite substitutes `import.meta.env.PROD` with `true` in the shipped, Vite-bundled core package (and
 * leaves it `false` in Vite dev). Outside a Vite build — the initial `tsc` pass and web-test-runner —
 * `import.meta.env` is undefined, so the guard returns false there.
 * @returns {boolean} `true` if the current build is a production build.
 */
export function umbIsProductionBuild(): boolean {
	return typeof import.meta.env !== 'undefined' && import.meta.env.PROD === true;
}
