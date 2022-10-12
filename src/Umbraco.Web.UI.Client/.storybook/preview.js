import '../src/core/context/context-provider.element';
import '../src/css/custom-properties.css';
import '../src/backoffice/components/backoffice-modal-container.element';
import '@umbraco-ui/uui';
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import '@umbraco-ui/uui-modal-sidebar';

import { html } from 'lit-html';
import { initialize, mswDecorator } from 'msw-storybook-addon';
import { setCustomElements } from '@storybook/web-components';

import customElementManifests from '../custom-elements.json';
import { UmbExtensionRegistry } from '../src/core/extension';
import { UmbDataTypeStore } from '../src/core/stores/data-type/data-type.store';
import { UmbDocumentTypeStore } from '../src/core/stores/document-type.store';
import { UmbNodeStore } from '../src/core/stores/node.store';
import { UmbPropertyEditorStore } from '../src/core/stores/property-editor/property-editor.store';
import { UmbPropertyEditorConfigStore } from '../src/core/stores/property-editor-config/property-editor-config.store';
import { UmbIconStore } from '../src/core/stores/icon/icon.store';
import { onUnhandledRequest } from '../src/mocks/browser';
import { handlers } from '../src/mocks/browser-handlers';
import { internalManifests } from '../src/temp-internal-manifests';
import { LitElement } from 'lit';
import { UmbModalService } from '../src/core/services/modal';

const extensionRegistry = new UmbExtensionRegistry();
internalManifests.forEach((manifest) => extensionRegistry.register(manifest));

class UmbStoryBookElement extends LitElement {
	_umbIconStore = new UmbIconStore();

	constructor() {
		super();
		this._umbIconStore.attach(this);
	}

	render() {
		return html`<slot></slot>`;
	}
}

customElements.define('umb-storybook', UmbStoryBookElement);

const storybookProvider = (story) => html` <umb-storybook>${story()}</umb-storybook> `;

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

const propertyEditorStoreProvider = (story) => html`
	<umb-context-provider key="umbPropertyEditorStore" .value=${new UmbPropertyEditorStore()}
		>${story()}</umb-context-provider
	>
`;

const propertyEditorConfigStoreProvider = (story) => html`
	<umb-context-provider key="umbPropertyEditorConfigStore" .value=${new UmbPropertyEditorConfigStore()}
		>${story()}</umb-context-provider
	>
`;

const modalServiceProvider = (story) => html`
	<umb-context-provider style="display: block; padding: 32px;" key="umbModalService" .value=${new UmbModalService()}>
		${story()}
		<umb-backoffice-modal-container></umb-backoffice-modal-container>
	</umb-context-provider>
`;

// Initialize MSW
initialize({ onUnhandledRequest });

// Provide the MSW addon decorator globally
export const decorators = [
	mswDecorator,
	storybookProvider,
	extensionRegistryProvider,
	nodeStoreProvider,
	dataTypeStoreProvider,
	documentTypeStoreProvider,
	propertyEditorStoreProvider,
	propertyEditorConfigStoreProvider,
	modalServiceProvider,
];

export const parameters = {
	options: {
		storySort: {
			method: 'alphabetical',
			includeNames: true,
		},
	},
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

setCustomElements(customElementManifests);
