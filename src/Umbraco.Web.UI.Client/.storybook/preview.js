import '../src/core/context/context-provider.element';
import '../src/css/custom-properties.css';
import '@umbraco-ui/uui';

import { html } from 'lit-html';
import { initialize, mswDecorator } from 'msw-storybook-addon';

import { UmbExtensionRegistry } from '../src/core/extension';
import { UmbDataTypeStore } from '../src/core/stores/data-type.store';
import { UmbDocumentTypeStore } from '../src/core/stores/document-type.store';
import { UmbNodeStore } from '../src/core/stores/node.store';
import { onUnhandledRequest } from '../src/mocks/browser';
import { handlers } from '../src/mocks/browser-handlers';
import { internalManifests } from '../src/temp-internal-manifests';

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
