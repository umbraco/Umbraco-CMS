import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from './property-type-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
@customElement('umb-property-type-workspace-editor')
export class UmbPropertyTypeWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	_isNew?: boolean;

	@state()
	_name?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.observe(context?.isNew, (isNew) => {
				this._isNew = isNew;
			});
			this.observe(context?.name, (name) => {
				this._name = name;
			});
			this.#workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	override render() {
		return this._isNew !== undefined
			? html`
					<umb-workspace-editor
						headline=${this.localize.term(
							this._isNew ? 'contentTypeEditor_addProperty' : 'contentTypeEditor_editProperty',
							[this._name],
						)}>
					</umb-workspace-editor>
				`
			: '';
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbPropertyTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-workspace-editor': UmbPropertyTypeWorkspaceEditorElement;
	}
}
