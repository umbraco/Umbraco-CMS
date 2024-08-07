/**
 *
 * @param udi
 */
export function getGuid(udi: string) {
	if (!udi.startsWith('umb://')) throw new Error('udi does not start with umb://');

	const withoutScheme = udi.replace('umb://', '');
	const withoutHost = withoutScheme.split('/')[1];
	if (withoutHost.length !== 32) throw new Error('udi is not 32 chars');

	return `${withoutHost.substring(0, 8)}-${withoutHost.substring(8, 12)}-${withoutHost.substring(12, 16)}-${withoutHost.substring(16, 20)}-${withoutHost.substring(20)}`;
}
