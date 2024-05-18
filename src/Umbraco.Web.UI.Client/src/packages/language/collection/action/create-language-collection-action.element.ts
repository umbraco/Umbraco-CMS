import { UMB_LANGUAGE_ENTITY_TYPE, UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_LANGUAGE_WORKSPACE_MODAL } from '../../workspace/language/language-workspace.modal-token.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';

@customElement('umb-create-language-collection-action')
export class UmbCreateLanguageCollectionActionElement extends UmbLitElement {
	@state()
	private _createPath = '';

	@state()
	private _currentView?: string;

	@state()
	private _rootPathName?: string;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_LANGUAGE_WORKSPACE_MODAL)
			.addAdditionalPath('language')
			.onSetup(() => {
				return { data: { entityType: UMB_LANGUAGE_ENTITY_TYPE, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._createPath = routeBuilder({});
			});

		this.consumeContext(UMB_COLLECTION_CONTEXT, (collectionContext) => {
			this.observe(collectionContext.view.currentView, (currentView) => {
				this._currentView = currentView?.meta.pathName;
			});
			this.observe(collectionContext.view.rootPathName, (rootPathName) => {
				this._rootPathName = rootPathName;
			});
		});
	}

	#getCreateUrl() {
		return (
			this._createPath.replace(`${this._rootPathName}`, `${this._rootPathName}/${this._currentView}`) +
			UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN.generateLocal({
				parentEntityType: UMB_LANGUAGE_ROOT_ENTITY_TYPE,
				parentUnique: null,
			})
		);
	}

	render() {
		return html` <uui-button color="default" href=${this.#getCreateUrl()} label="Create" look="outline"></uui-button> `;
	}
}

export { UmbCreateLanguageCollectionActionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-language-collection-action': UmbCreateLanguageCollectionActionElement;
	}
}
