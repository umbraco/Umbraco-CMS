import '@umbraco-ui/uui';
import '@umbraco-ui/uui-css/dist/uui-css.css';
import { html } from 'lit-html';

import { initialize, mswDecorator } from 'msw-storybook-addon';

import { onUnhandledRequest } from '../src/mocks/browser';
import { handlers } from '../src/mocks/handlers';

import { UmbExtensionRegistry } from '../src/core/extension';
import { internalManifests } from '../src/temp-internal-manifests';

import '../src/core/context/context-provider.element';
import { UmbNodeStore } from '../src/core/stores/node.store';
import { UmbDataTypeStore } from '../src/core/stores/data-type.store';
import { UmbDocumentTypeStore } from '../src/core/stores/document-type.store';

const extensionRegistry = new UmbExtensionRegistry();
internalManifests.forEach((manifest) => extensionRegistry.register(manifest));

const extensionRegistryProvider = (story) => html`
	<umb-context-provider key="umbExtensionRegistry" .value=${extensionRegistry}>${story()}</umb-context-provider>
`;

const nodeStoreProvider = (story) => html`
	<umb-context-provider key="umbNodeStore" .value=${new UmbNodeStore()}>${story()}</umb-context-provider>
`;

const dataTypeStoreProvider = (story) => html`
	<umb-context-provider key="umbDataTypeStore" .value=${new UmbDataTypeStore()}>${story()}</umb-context-provider>
`;

const documentTypeStoreProvider = (story) => html`
	<umb-context-provider key="umbDocumentTypeStore" .value=${new UmbDocumentTypeStore()}
		>${story()}</umb-context-provider
	>
`;

// Initialize MSW
initialize({ onUnhandledRequest });

// Provide the MSW addon decorator globally
export const decorators = [
	mswDecorator,
	extensionRegistryProvider,
	nodeStoreProvider,
	dataTypeStoreProvider,
	documentTypeStoreProvider,
];

export const parameters = {
	actions: { argTypesRegex: '^on.*' },
	controls: {
		expanded: true,
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
	msw: {
		handlers: {
			global: handlers,
		},
	},
};
