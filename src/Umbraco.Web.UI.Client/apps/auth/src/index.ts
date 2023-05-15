import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-registry';
import { manifests as externalLoginProviders } from './external-login-providers/manifests';

import '@umbraco-ui/uui-css/dist/uui-css.css';
import '@umbraco-cms/backoffice/css';
import '@umbraco-ui/uui';

import './login.element';
import 'element-internals-polyfill';

const registerExtensions = (manifests: Array<ManifestTypes>) => {
	manifests.forEach((manifest) => {
		if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
		umbExtensionsRegistry.register(manifest);
	});
};

registerExtensions([...externalLoginProviders]);
