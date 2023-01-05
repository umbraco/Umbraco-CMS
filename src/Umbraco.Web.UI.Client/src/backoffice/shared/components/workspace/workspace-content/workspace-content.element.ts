import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import type { UmbWorkspaceContentContext } from './workspace-content.context';
import type { DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import '../workspace-layout/workspace-layout.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/edit/workspace-view-content-edit.element';
import './views/info/workspace-view-content-info.element';
import type { UmbNodeStoreBase } from '@umbraco-cms/stores/store';
import { UmbLitElement } from '@umbraco-cms/element';

type ContentTypeTypes = DocumentDetails | MediaDetails;

/**
 * TODO: IMPORTANT TODO: Get rid of the content workspace. Instead we aim to get separate components that can be composed by each workspace.
 * Example. Document Workspace would use a Variant-component(variant component would talk directly to the workspace-context)
 * As well breadcrumbs etc.
 * 
 */
@customElement('umb-workspace-content')
export class UmbWorkspaceContentElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
			}

			#footer {
				margin: 0 var(--uui-size-layout-1);
			}
		`,
	];

	// TODO: is this used for anything?
	@property()
	alias!: string;

	// TODO: use a NodeDetails type here:
	@state()
	_content?: ContentTypeTypes;

	private _workspaceContext?: UmbWorkspaceContentContext<ContentTypeTypes, UmbNodeStoreBase<ContentTypeTypes>>;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (instance) => {
			this._workspaceContext = instance;
			this._observeWorkspace();
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	private async _observeWorkspace() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (data) => {
			this._content = data;
		});
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;

		// TODO: Set value.
		const property = this._content?.properties.find((x) => x.alias === target.alias);
		if (property) {
			this._setPropertyValue(property.alias, target.value);
		} else {
			console.error('property was not found', target.alias);
		}
	};

	// TODO: How do we ensure this is a change of this document and not nested documents? Should the event be stopped at this spot at avoid such.
	private _setPropertyValue(alias: string, value: unknown) {
		this._content?.data.forEach((data) => {
			if (data.alias === alias) {
				data.value = value;
			}
		});
	}

	render() {
		return html`
			<umb-workspace-layout alias=${this.alias}>
				<div id="header" slot="header">
					<umb-variant-selector></umb-variant-selector>
				</div>

				<div id="footer" slot="footer">Breadcrumbs</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbWorkspaceContentElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-content': UmbWorkspaceContentElement;
	}
}
