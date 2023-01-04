import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { ManifestTreeItemAction } from '@umbraco-cms/models';

import '../../../shared/components/tree/navigator/tree-navigator.element';
import { UmbDataTypeStore } from 'src/backoffice/settings/data-types/data-type.store';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-tree-data-types')
export class UmbTreeDataTypesElement extends UmbLitElement {
	constructor() {
		super();

		this._registerTreeItemActions();

		// TODO: how do we best expose the tree api to the tree navigator element?
		this.consumeContext('umbDataTypeStore', (dataTypeStore: UmbDataTypeStore) => {
			this.provideContext('umbTreeStore', dataTypeStore);
		});
	}

	private _registerTreeItemActions() {
		const dashboards: Array<ManifestTreeItemAction> = [];

		dashboards.forEach((dashboard) => {
			if (umbExtensionsRegistry.isRegistered(dashboard.alias)) return;
			umbExtensionsRegistry.register(dashboard);
		});
	}

	render() {
		return html`<umb-tree-navigator></umb-tree-navigator>`;
	}
}

export default UmbTreeDataTypesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-data-types': UmbTreeDataTypesElement;
	}
}
