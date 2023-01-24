import { ManifestTypes } from '@umbraco-cms/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { manifests as externalLoginProviders } from './external-login-providers/manifests';

import './login.element';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...externalLoginProviders]);
