import { UmbApi } from "../models/api.interface.js";
import { ManifestApi, ManifestElementAndApi } from "../types/base.types.js";
import { loadManifestApi } from "./load-manifest-api.function.js";

export async function createManifestApi<ApiType extends UmbApi = UmbApi>(manifest: ManifestApi<ApiType> | ManifestElementAndApi<any, ApiType>, constructorArguments: unknown[] = []): Promise<ApiType | undefined> {

	if(manifest.api) {
		const apiConstructor = await loadManifestApi<ApiType>(manifest.api);
		if(apiConstructor) {
			return new apiConstructor(constructorArguments);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed instantiate a API class via the extension manifest property 'api', using either a 'api' or 'default' export`,
				manifest
			);
		}
	}

	if(manifest.js) {
		const apiConstructor2 = await loadManifestApi<ApiType>(manifest.js);
		if(apiConstructor2) {
			return new apiConstructor2(constructorArguments);
		} else {
			console.error(
				`-- Extension of alias "${manifest.alias}" did not succeed instantiate a API class via the extension manifest property 'js', using either a 'api' or 'default' export`,
				manifest
			);
		}
	}

	console.error(
		`-- Extension of alias "${manifest.alias}" did not succeed creating an api class instance, missing a JavaScript file via the 'api' or 'js' property.`,
		manifest
	);

	return undefined;
}
