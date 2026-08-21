import { UMB_DATA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from './data-type-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from '@umbraco-cms/backoffice/schema-lockdown';
/**
 * @element umb-data-type-workspace-editor
 * @description - Element for displaying the Data Type Workspace edit route.
 */
@customElement('umb-data-type-workspace-editor')
export class UmbDataTypeWorkspaceEditorElement extends UmbLitElement {
	// Restricted until the schema lockdown matrix confirms the operation is allowed (safe default).
	@state()
	private _isRestricted = true;

	#datasetContext?: { setReadOnly?: (value: boolean) => void };
	#schemaLockdownContext?: typeof UMB_SCHEMA_LOCKDOWN_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#datasetContext = workspaceContext?.createPropertyDatasetContext(this) as unknown as {
				setReadOnly?: (value: boolean) => void;
			};
			this.#updateReadOnly();
		});

		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.#schemaLockdownContext = context;
			this.observe(context?.state, () => {
				this._isRestricted = this.#schemaLockdownContext?.isAllowed(UMB_DATA_TYPE_ENTITY_TYPE, 'update') !== true;
				this.#updateReadOnly();
			});
		});
	}

	// When the schema is locked down the data type is read-only. Setting the property dataset context
	// read-only cascades to every config property editor without wiring each one individually.
	#updateReadOnly() {
		this.#datasetContext?.setReadOnly?.(this._isRestricted);
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable
					slot="header"
					?readonly=${this._isRestricted}></umb-workspace-header-name-editable>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbDataTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace-editor': UmbDataTypeWorkspaceEditorElement;
	}
}
