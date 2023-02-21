export function buildUdi(entityType: string, guid: string) {
	return `umb://${entityType}/${guid.replace(/-/g, '')}`;
}

export function getKeyFromUdi(udi: string) {
	if (typeof udi !== 'string') {
		throw 'udi is not a string';
	}
	if (!udi.startsWith('umb://')) {
		throw 'udi does not start with umb://';
	}
	const withoutScheme = udi.substring('umb://'.length);
	const withoutHost = withoutScheme.substring(withoutScheme.indexOf('/') + 1).trim();

	if (withoutHost.length !== 32) {
		throw 'udi is not 32 chars';
	}

	return `${withoutHost.substring(0, 8)}-${withoutHost.substring(8, 12)}-${withoutHost.substring(
		12,
		16
	)}-${withoutHost.substring(16, 20)}-${withoutHost.substring(20)}`;
}
