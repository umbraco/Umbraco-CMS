/**
 * Returns true if the current build is a production build, false otherwise.
 *
 * Vite substitutes `import.meta.env.PROD` with `true` in the shipped core bundle, so this is a reliable
 * way to detect production builds. In development builds and in the initial tsc pass (e.g. for web-test-runner),
 * `import.meta.env` is undefined, so we guard and return false there.
 * @returns {boolean} `true` if the current build is a production build.
 */
export function umbIsProductionBuild(): boolean {
	return typeof import.meta.env !== 'undefined' && import.meta.env.PROD === true;
}
