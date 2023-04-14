import '@umbraco-ui/uui-css/dist/uui-css.css';
import '../src/core/css/custom-properties.css';

import 'element-internals-polyfill';
import '@umbraco-ui/uui';

import { html } from 'lit';
import { initialize, mswDecorator } from 'msw-storybook-addon';
import { setCustomElements } from '@storybook/web-components';

import { UmbDataTypeStore } from '../src/backoffice/settings/data-types/repository/data-type.store.ts';
import { UmbDocumentTypeStore } from '../src/backoffice/documents/document-types/repository/document-type.store.ts';
import { UmbDocumentStore } from '../src/backoffice/documents/documents/repository/document.store.ts';
import { UmbDocumentTreeStore } from '../src/backoffice/documents/documents/repository/document.tree.store.ts';

import customElementManifests from '../custom-elements.json';
import { UmbIconStore } from '../src/core/stores/icon/icon.store';
import { onUnhandledRequest } from '../src/core/mocks/browser';
import { handlers } from '../src/core/mocks/browser-handlers';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext } from '../libs/modal';
import { UmbLitElement } from '../src/core/lit-element';

import { umbExtensionsRegistry } from '../libs/extensions-api';

import '../src/core/context-provider/context-provider.element';
import '../src/core/controller-host/controller-host-test.element';
import '../src/backoffice/shared/components';

import { manifests as documentManifests } from '../src/backoffice/documents';

class UmbStoryBookElement extends UmbLitElement {
	_umbIconStore = new UmbIconStore();

	constructor() {
		super();
		this._umbIconStore.attach(this);
		this._registerExtensions(documentManifests);
		this.provideContext(UMB_MODAL_CONTEXT_TOKEN, new UmbModalContext(this));
	}

	_registerExtensions(manifests) {
		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html`<slot></slot>
			<umb-backoffice-modal-container></umb-backoffice-modal-container> `;
	}
}

customElements.define('umb-storybook', UmbStoryBookElement);

const storybookProvider = (story) => html` <umb-storybook>${story()}</umb-storybook> `;

const dataTypeStoreProvider = (story) => html`
	<umb-controller-host-test .create=${(host) => new UmbDataTypeStore(host)}>${story()}</umb-controller-host-test>
`;

const documentTypeStoreProvider = (story) => html`
	<umb-controller-host-test .create=${(host) => new UmbDocumentTypeStore(host)}>${story()}</umb-controller-host-test>
`;

const documentStoreProvider = (story) => html`
	<umb-controller-host-test .create=${(host) => new UmbDocumentStore(host)}>${story()}</umb-controller-host-test>
`;

const documentTreeStoreProvider = (story) => html`
	<umb-controller-host-test .create=${(host) => new UmbDocumentTreeStore(host)}>${story()}</umb-controller-host-test>
`;

// Initialize MSW
initialize({ onUnhandledRequest });

// Provide the MSW addon decorator globally
export const decorators = [
	mswDecorator,
	storybookProvider,
	documentStoreProvider,
	documentTreeStoreProvider,
	dataTypeStoreProvider,
	documentTypeStoreProvider,
];

export const parameters = {
	options: {
		storySort: {
			method: 'alphabetical',
			includeNames: true,
			order: [
				'Guides',
				[
					'Getting started',
					'Extending the Backoffice',
					[
						'Intro',
						'Registration',
						'Header Apps',
						'Sections',
						['Intro', 'Sidebar', 'Views', '*'],
						'Entity Actions',
						'Workspaces',
						['Intro', 'Views', 'Actions', '*'],
						'Property Editors',
						'Repositories',
						'*',
					],
					'*',
				],
				'*',
			],
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
	backgrounds: {
		default: 'Greyish',
		values: [
			{
				name: 'Greyish',
				value: '#F3F3F5',
			},
			{
				name: 'White',
				value: '#ffffff',
			},
		],
	},
};

setCustomElements(customElementManifests);
