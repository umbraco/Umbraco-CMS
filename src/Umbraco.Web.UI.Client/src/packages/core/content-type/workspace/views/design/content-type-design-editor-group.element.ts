import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbContentTypeContainerStructureHelper,
	UmbContentTypeModel,
	UmbPropertyTypeContainerModel,
} from '@umbraco-cms/backoffice/content-type';

import './content-type-design-editor-properties.element.js';

@customElement('umb-content-type-design-editor-group')
export class UmbContentTypeWorkspaceViewEditGroupElement extends UmbLitElement {
	@property({ attribute: false })
	public set group(value: UmbPropertyTypeContainerModel | undefined) {
		if (value === this._group) return;
		this._group = value;
		this._groupId = value?.id;
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
	_groupId?: string;

	@state()
	_hasOwnerContainer?: boolean;

	@state()
	_inherited?: boolean;

	#checkInherited() {
		if (this.groupStructureHelper && this.group) {
			// Check is this container matches with any other group. If so it is inherited aka. merged with others. [NL]
			if (this.group.name) {
				// We can first match with something if we have a name [NL]
				this.observe(
					this.groupStructureHelper.containersByNameAndType(this.group.name, 'Group'),
					(containers) => {
						const hasAOwnerContainer = containers.some((con) =>
							this.groupStructureHelper!.isOwnerChildContainer(con.id),
						);
						const pureOwnerContainer = hasAOwnerContainer && containers.length === 1;

						// TODO: Check if requstUpdate is needed here, I do not think it is when i added it, but I just wanted to be safe when debugging [NL]
						const oldHasOwnerContainer = this._hasOwnerContainer;
						const oldInherited = this._inherited;
						this._hasOwnerContainer = hasAOwnerContainer;
						this._inherited = !pureOwnerContainer;
						this.requestUpdate('_hasOwnerContainer', oldHasOwnerContainer);
						this.requestUpdate('_inherited', oldInherited);
					},
					'observeGroupContainers',
				);
			} else {
				// We use name match to determine inheritance, so no name cannot inherit.
				this._inherited = false;
				this._hasOwnerContainer = true;
				this.removeUmbControllerByAlias('observeGroupContainers');
			}
		}
	}

	_singleValueUpdate(propertyName: string, value: string | number | boolean | null | undefined) {
		if (!this._groupStructureHelper || !this.group) return;

		const partialObject = {} as any;
		partialObject[propertyName] = value;

		this._groupStructureHelper.partialUpdateContainer(this.group.id, partialObject);
	}

	#renameGroup(e: InputEvent) {
		if (!this.groupStructureHelper || !this._group) return;
		let newName = (e.target as HTMLInputElement).value;
		const changedName = this.groupStructureHelper
			.getStructureManager()!
			.makeContainerNameUniqueForOwnerContentType(this._group.id, newName, 'Group', this._group.parent?.id ?? null);
		if (changedName) {
			newName = changedName;
		}
		this._singleValueUpdate('name', newName);
		(e.target as HTMLInputElement).value = newName;
	}

	#blurGroup(e: InputEvent) {
		if (!this.groupStructureHelper || !this._group) return;
		const newName = (e.target as HTMLInputElement).value;
		if (newName === '') {
			const changedName = this.groupStructureHelper
				.getStructureManager()!
				.makeEmptyContainerName(this._group.id, 'Group', this._group.parent?.id ?? null);
			this._singleValueUpdate('name', changedName);
			(e.target as HTMLInputElement).value = changedName;
		}
	}

	render() {
		return this._inherited !== undefined && this._groupId
			? html`
					<uui-box>
						${this.#renderContainerHeader()}
						<umb-content-type-design-editor-properties
							container-id=${this._groupId}></umb-content-type-design-editor-properties>
					</uui-box>
				`
			: '';
	}

	#renderContainerHeader() {
		return html`<div slot="header">
			<div>
				<uui-icon name=${this._inherited ? 'icon-merge' : 'icon-navigation'}></uui-icon>
				<uui-input
					label=${this.localize.term('contentTypeEditor_group')}
					placeholder=${this.localize.term('placeholders_entername')}
					.value=${this._group!.name}
					?disabled=${!this._hasOwnerContainer}
					@change=${this.#renameGroup}
					@blur=${this.#blurGroup}></uui-input>
			</div>
			${this.sortModeActive
				? html` <uui-input
						type="number"
						label=${this.localize.term('sort_sortOrder')}
						@change=${(e: UUIInputEvent) =>
							this._singleValueUpdate('sortOrder', parseInt(e.target.value as string) || 0)}
						.value=${this.group!.sortOrder ?? 0}
						?disabled=${!this._hasOwnerContainer}></uui-input>`
				: ''}
		</div> `;
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
		'umb-content-type-design-editor-group': UmbContentTypeWorkspaceViewEditGroupElement;
	}
}
