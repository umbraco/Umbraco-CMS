import type { JsLoaderProperty } from "../types/utils.js";

export async function loadManifestPlainCss<CssType = string>(property: JsLoaderProperty<CssType>): Promise<CssType | undefined> {
	const propType = typeof property;
	if(propType === 'function') {
		const result = await (property as (Exclude<(typeof property), string>))();
		if(result != null) {
			return result;
		}
	} else if(propType === 'string') {
		// Import string
		const result = await (import(/* @vite-ignore */ property as string) as unknown as CssType);
		if(result != null) {
			return result;
		}
	}
	return undefined;
}