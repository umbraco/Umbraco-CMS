import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import type { UmbWorkspaceEntityElement } from '../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbVariantId } from '../../../shared/variants/variant-id.class';
import { ActiveVariant, UmbDocumentWorkspaceContext } from './document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-document-workspace')
export class UmbDocumentWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	private _workspaceContext: UmbDocumentWorkspaceContext = new UmbDocumentWorkspaceContext(this);

	@state()
	_unique?: string;

	@state()
	_workspaceSplitViews: Array<ActiveVariant> = [];

	constructor() {
		super();
		this.observe(this._workspaceContext.activeVariantsInfo, (variants) => {
			this._workspaceSplitViews = variants;
		});
	}

	public async load(entityKey: string) {
		const data = await this._workspaceContext.load(entityKey);
		this._gotDocumentData(data);
	}

	public async create(parentKey: string | null) {
		const data = await this._workspaceContext.createScaffold(parentKey);
		this._gotDocumentData(data);
	}

	private _gotDocumentData(data: any) {
		if (data && data.variants && data.variants.length > 0) {
			// TODO: consider making a DocumentVariant object for this VariantId:
			this._workspaceContext.setActiveVariant(0, new UmbVariantId(data.variants[0]));
			this._unique = data.key;
		} else {
			// Fail beautifully?
		}
	}

	render() {
		return this._unique
			? repeat(
					this._workspaceSplitViews,
					(view) => view.index,
					(view) => html`
						<umb-workspace-variant-content alias="Umb.Workspace.Document" .splitViewIndex=${view.index}>
							<umb-workspace-action-menu
								slot="action-menu"
								entity-type="document"
								unique="${this._unique!}"></umb-workspace-action-menu>
						</umb-workspace-variant-content>
					`
			  )
			: nothing;
	}
}

export default UmbDocumentWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace': UmbDocumentWorkspaceElement;
	}
}
