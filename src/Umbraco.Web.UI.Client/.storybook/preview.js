import '../src/core/css/custom-properties.css';
import '@umbraco-ui/uui-css/dist/uui-css.css';

import '@umbraco-ui/uui';
import '@umbraco-ui/uui-modal';
import '@umbraco-ui/uui-modal-container';
import '@umbraco-ui/uui-modal-dialog';
import '@umbraco-ui/uui-modal-sidebar';

import { html } from 'lit-html';
import { initialize, mswDecorator } from 'msw-storybook-addon';
import { setCustomElements } from '@storybook/web-components';

import customElementManifests from '../custom-elements.json';
import { UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN, UmbDataTypeStore } from '../src/backoffice/settings/data-types/data-type.detail.store';
import { UmbDocumentTypeStore } from '../src/backoffice/documents/document-types/document-type.store';
import { UmbIconStore } from '../src/core/stores/icon/icon.store';
import { onUnhandledRequest } from '../src/core/mocks/browser';
import { handlers } from '../src/core/mocks/browser-handlers';
import { LitElement } from 'lit';
import { UmbModalService } from '../src/core/modal';

// TODO: Fix storybook manifest registrations.

import { umbExtensionsRegistry } from '../src/core/extensions-registry';

import '../src/core/context-api/provide/context-provider.element';
import '../src/backoffice/shared/components/backoffice-frame/backoffice-modal-container.element';
import '../src/backoffice/shared/components/code-block/code-block.element';

class UmbStoryBookElement extends LitElement {
	_umbIconStore = new UmbIconStore();

	constructor() {
		super();
		this._umbIconStore.attach(this);
	}

	_registerExtensions(manifests) {
		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html`<slot></slot>`;
	}
}

customElements.define('umb-storybook', UmbStoryBookElement);

const storybookProvider = (story) => html` <umb-storybook>${story()}</umb-storybook> `;

const dataTypeStoreProvider = (story) => html`
	<umb-context-provider key=${UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString()} .value=${new UmbDataTypeDetailStore()}>${story()}</umb-context-provider>
`;

const documentTypeStoreProvider = (story) => html`
	<umb-context-provider key="umbDocumentTypeStore" .value=${new UmbDocumentTypeStore()}
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
	dataTypeStoreProvider,
	documentTypeStoreProvider,
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
