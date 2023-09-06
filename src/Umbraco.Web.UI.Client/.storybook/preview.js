import '@umbraco-ui/uui-css/dist/uui-css.css';
import '../src/css/umb-css.css';

import 'element-internals-polyfill';
import '@umbraco-ui/uui';

import { html } from 'lit';
import { setCustomElements } from '@storybook/web-components';

import { startMockServiceWorker } from '../src/mocks';

import { UMB_MODAL_CONTEXT_TOKEN, UmbModalManagerContext } from '../src/packages/core/modal';
import { UmbDataTypeStore } from '../src/packages/settings/data-types/repository/data-type.store.ts';
import { UmbDocumentStore } from '../src/packages/documents/documents/repository/document.store.ts';
import { UmbDocumentTreeStore } from '../src/packages/documents/documents/repository/document.tree.store.ts';
import { UmbDocumentTypeStore } from '../src/packages/documents/document-types/repository/document-type.store.ts';
import { umbExtensionsRegistry } from '../src/packages/core/extension-registry';
import { UmbIconRegistry } from '../src/shared/icon-registry/icon.registry';
import { UmbLitElement } from '../src/shared/lit-element';
import { umbLocalizationRegistry } from '../src/packages/core/localization';
import customElementManifests from '../dist-cms/custom-elements.json';

import '../src/libs/context-api/provide/context-provider.element';
import '../src/libs/controller-api/controller-host-initializer.element.ts';
import '../src/packages/core/components';

import { manifests as documentManifests } from '../src/packages/documents';
import { manifests as localizationManifests } from '../src/packages/core/localization/manifests';

// MSW
startMockServiceWorker({ serviceWorker: { url: (import.meta.env.VITE_BASE_PATH ?? '/') + 'mockServiceWorker.js' } });

class UmbStoryBookElement extends UmbLitElement {
	_umbIconRegistry = new UmbIconRegistry();

	constructor() {
		super();
		this._umbIconRegistry.attach(this);
		this._registerExtensions(documentManifests);
		this.provideContext(UMB_MODAL_CONTEXT_TOKEN, new UmbModalManagerContext(this));

		this._registerExtensions(localizationManifests);
		umbLocalizationRegistry.loadLanguage('en-us'); // register default language
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
	<umb-controller-host-initializer .create=${(host) => new UmbDataTypeStore(host)}
		>${story()}</umb-controller-host-initializer
	>
`;

const documentTypeStoreProvider = (story) => html`
	<umb-controller-host-initializer .create=${(host) => new UmbDocumentTypeStore(host)}
		>${story()}</umb-controller-host-initializer
	>
`;

const documentStoreProvider = (story) => html`
	<umb-controller-host-initializer .create=${(host) => new UmbDocumentStore(host)}
		>${story()}</umb-controller-host-initializer
	>
`;

const documentTreeStoreProvider = (story) => html`
	<umb-controller-host-initializer .create=${(host) => new UmbDocumentTreeStore(host)}
		>${story()}</umb-controller-host-initializer
	>
`;

// Provide the MSW addon decorator globally
export const decorators = [
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
