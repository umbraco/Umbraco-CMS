import { v4 as uuidv4 } from 'uuid';
import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import type { ManifestTree } from '@umbraco-cms/models';

@customElement('umb-sidebar-menu-item')
export class UmbSidebarMenuItem extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _key = uuidv4();

	_manifest?: ManifestTree;
	@property({ type: Object, attribute: false })
	public get manifest(): ManifestTree | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestTree | undefined) {
		const oldVal = this._manifest;
		this._manifest = value;
		this.requestUpdate('manifest', oldVal);

		if (value) {
			this._treeItem = {
				key: this._key,
				name: value.meta.label ?? value.name,
				icon: value.meta.icon,
				type: value.meta.rootNodeEntityType,
				hasChildren: false,
				parentKey: null,
			};
		}
	}

	@state()
	private _treeItem: Entity;

	render() {
		return html`<umb-tree-item .treeItem=${this._treeItem}></umb-tree-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-sidebar-menu-item': UmbSidebarMenuItem;
	}
}
