import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbContentTypeContainerStructureHelper,
	UmbContentTypeModel,
	UmbPropertyTypeContainerModel,
} from '@umbraco-cms/backoffice/content-type';

import './content-type-workspace-view-edit-properties.element.js';

@customElement('umb-content-type-workspace-view-edit-group')
export class UmbContentTypeWorkspaceViewEditGroupElement extends UmbLitElement {
	private _ownerGroupId?: string | null;

	/*
	@property({ type: String })
	public get ownerGroupId(): string | null | undefined {
		return this._ownerGroupId;
	}
	public set ownerGroupId(value: string | null | undefined) {
		if (value === this._ownerGroupId) return;
		const oldValue = this._ownerGroupId;
		this._ownerGroupId = value;
		this.requestUpdate('ownerGroupId', oldValue);
	}
	private _groupName?: string | undefined;

	@property({ type: String })
	public get groupName(): string | undefined {
		return this._groupName;
	}
	public set groupName(value: string | undefined) {
		if (value === this._groupName) return;
		const oldValue = this._groupName;
		this._groupName = value;
		this.requestUpdate('groupName', oldValue);
	}
	*/

	@property({ attribute: false })
	public set group(value: UmbPropertyTypeContainerModel | undefined) {
		if (value === this._group) return;
		this._group = value;
		this.#checkInherited();
	}
	public get group(): UmbPropertyTypeContainerModel | undefined {
		return this._group;
	}
	private _group?: UmbPropertyTypeContainerModel | undefined;

	@property({ attribute: false })
	public set groupStructureHelper(value: UmbContentTypeContainerStructureHelper<UmbContentTypeModel> | undefined) {
		if (value === this._groupStructureHelper) return;
		this._groupStructureHelper = value;
		this.#checkInherited();
	}
	public get groupStructureHelper(): UmbContentTypeContainerStructureHelper<UmbContentTypeModel> | undefined {
		return this._groupStructureHelper;
	}
	private _groupStructureHelper?: UmbContentTypeContainerStructureHelper<UmbContentTypeModel> | undefined;

	@property({ type: Boolean, attribute: 'sort-mode-active', reflect: true })
	sortModeActive = false;

	@state()
	_inherited?: boolean;

	constructor() {
		super();
	}

	#checkInherited() {
		if (this.groupStructureHelper && this.group) {
			this._inherited = !this.groupStructureHelper.isOwnerChildContainer(this.group.id);
		}
	}

	/*
	_partialUpdate(partialObject: Partial<UmbPropertyTypeContainerModel>) {
		this.dispatchEvent(new CustomEvent('umb:partial-group-update', { detail: partialObject }));
	}
	*/

	_singleValueUpdate(propertyName: string, value: string | number | boolean | null | undefined) {
		const partialObject = {} as any;
		partialObject[propertyName] = value;

		this.dispatchEvent(new CustomEvent('umb:partial-group-update', { detail: partialObject }));
	}

	render() {
		return this._inherited !== undefined
			? html`
					<uui-box>
						${this.#renderContainerHeader()}
						<umb-content-type-workspace-view-edit-properties
							container-id=${this.group!.id}
							container-type="Group"
							container-name=${this.group!.name ?? ''}></umb-content-type-workspace-view-edit-properties>
					</uui-box>
				`
			: '';
	}

	#renderContainerHeader() {
		if (this.sortModeActive) {
			return html`<div slot="header">
				<div>
					<uui-icon name=${this._inherited ? 'icon-merge' : 'icon-navigation'}></uui-icon>
					${this.#renderInputGroupName()}
				</div>
				<uui-input
					type="number"
					label=${this.localize.term('sort_sortOrder')}
					@change=${(e: UUIInputEvent) => this._singleValueUpdate('sortOrder', parseInt(e.target.value as string) || 0)}
					.value=${this.group!.sortOrder ?? 0}
					?disabled=${this._inherited}></uui-input>
			</div> `;
		} else {
			return html`<div slot="header">
				${this._inherited ? html`<uui-icon name="icon-merge"></uui-icon>` : this.#renderInputGroupName()}
			</div> `;
		}
	}

	#renderInputGroupName() {
		return html`<uui-input
			label=${this.localize.term('contentTypeEditor_group')}
			placeholder=${this.localize.term('placeholders_entername')}
			.value=${this.group!.name}
			@change=${(e: InputEvent) => {
				const newName = (e.target as HTMLInputElement).value;
				this._singleValueUpdate('name', newName);
			}}></uui-input>`;
	}

	static styles = [
		css`
			:host([drag-placeholder]) {
				opacity: 0.5;
			}

			:host([drag-placeholder]) > * {
				visibility: hidden;
			}

			div[slot='header'] {
				flex: 1;
				display: flex;
				align-items: center;
				justify-content: space-between;
			}

			div[slot='header'] > div {
				display: flex;
				align-items: center;
				gap: var(--uui-size-3);
			}

			uui-input[type='number'] {
				max-width: 75px;
			}

			:host([sort-mode-active]) div[slot='header'] {
				cursor: grab;
			}
		`,
	];
}

export default UmbContentTypeWorkspaceViewEditGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-workspace-view-edit-group': UmbContentTypeWorkspaceViewEditGroupElement;
	}
}
