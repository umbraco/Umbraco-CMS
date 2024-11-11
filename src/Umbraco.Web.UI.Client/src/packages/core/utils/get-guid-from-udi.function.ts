/**
 * Get the guid from a UDI.
 * @example getGuidFromUdi('umb://document/4f058f8b1f7e4f3e8b4b6b4b4b6b4b6b') // '4f058f8b-1f7e-4f3e-8b4b-6b4b4b6b4b6b'
 * @param {string} udi The UDI to get the guid from.
 * @returns {string} The guid from the UDI.
 */
export function getGuidFromUdi(udi: string) {
	if (!udi.startsWith('umb://')) throw new Error('udi does not start with umb://');

	const withoutScheme = udi.replace('umb://', '');
	const withoutHost = withoutScheme.split('/')[1];
	if (withoutHost.length !== 32) throw new Error('udi is not 32 chars');

	return `${withoutHost.substring(0, 8)}-${withoutHost.substring(8, 12)}-${withoutHost.substring(12, 16)}-${withoutHost.substring(16, 20)}-${withoutHost.substring(20)}`;
}
