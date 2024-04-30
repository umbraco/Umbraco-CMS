import { mime } from '@umbraco-cms/backoffice/external/mime-types';

export function getExtensionFromMime(mimeType: string): string | undefined {
	const extension = mime.extension(mimeType);
	if (!extension) return; // extension doesn't exist.
	return extension;
}
