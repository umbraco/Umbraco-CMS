import type { UmbContentTypeModel, UmbPropertyTypeContainerModel } from '../../../types.js';
import type { UmbContentTypeContainerStructureHelper } from '../../../structure/index.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

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

	@property({ attribute: false })
	editContentTypePath?: string;

	@state()
	_groupId?: string;

	@state()
	_hasOwnerContainer?: boolean;

	@state()
	_inherited?: boolean;

	@state()
	_inheritedFrom?: Array<UmbContentTypeModel>;

	#checkInherited() {
		if (this.groupStructureHelper && this.group) {
			// Check is this container matches with any other group. If so it is inherited aka. merged with others. [NL]
			if (this.group.name) {
				// We can first match with something if we have a name [NL]
				this.observe(
					this.groupStructureHelper.containersByNameAndType(this.group.name, 'Group'),
					(containers) => {
						const ownerContainer = containers.find((con) => this.groupStructureHelper!.isOwnerChildContainer(con.id));
						const hasAOwnerContainer = !!ownerContainer;
						const pureOwnerContainer = hasAOwnerContainer && containers.length === 1;

						// TODO: Check if requestUpdate is needed here, I do not think it is when i added it, but I just wanted to be safe when debugging [NL]
						const oldHasOwnerContainer = this._hasOwnerContainer;
						const oldInherited = this._inherited;
						const oldInheritedFrom = this._inheritedFrom;
						this._hasOwnerContainer = hasAOwnerContainer;
						this._inherited = !pureOwnerContainer;
						this._inheritedFrom = containers
							.filter((con) => con.id !== ownerContainer?.id)
							.map((con) => this.groupStructureHelper!.getContentTypeOfContainer(con.id))
							.filter((contentType) => contentType !== undefined) as Array<UmbContentTypeModel>;
						this.requestUpdate('_hasOwnerContainer', oldHasOwnerContainer);
						this.requestUpdate('_inherited', oldInherited);
						this.requestUpdate('_inheritedFrom', oldInheritedFrom);
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

	async #requestRemove(e: Event) {
		e.preventDefault();
		e.stopImmediatePropagation();
		if (!this.groupStructureHelper || !this._group) return;

		// TODO: Do proper localization here: [NL]
		await umbConfirmModal(this, {
			headline: `${this.localize.term('actions_delete')} group`,
			content: html`<umb-localize key="contentTypeEditor_confirmDeleteGroupMessage" .args=${[
				this._group.name ?? this._group.id,
			]}>
					Are you sure you want to delete the group <strong>${this._group.name ?? this._group.id}</strong>
				</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		});

		this.groupStructureHelper.removeContainer(this._group.id);
	}

	override render() {
		if (this._inherited === undefined || !this._groupId) return nothing;
		return html`
			<uui-box>
				${this.#renderContainerHeader()}
				<umb-content-type-design-editor-properties
					.editContentTypePath=${this.editContentTypePath}
					.containerId=${this._groupId}></umb-content-type-design-editor-properties>
			</uui-box>
		`;
	}

	// TODO: impl UMB_EDIT_DOCUMENT_TYPE_PATH_PATTERN, but we need either a generic type or a way to get the path pattern.... [NL]
	#renderContainerHeader() {
		return html`
			<div slot="header" class="drag-handle">
				<div>
					${when(
						this.sortModeActive && this._hasOwnerContainer,
						() => html`<uui-icon name="icon-navigation"></uui-icon>`,
					)}
					<uui-input
						id="group-name"
						label=${this.localize.term('contentTypeEditor_group')}
						placeholder=${this.localize.term('placeholders_entername')}
						.value=${this._group!.name}
						?disabled=${!this._hasOwnerContainer}
						@blur=${this.#blurGroup}
						@change=${this.#renameGroup}
						${this._group!.name === '' ? umbFocus() : nothing}
						auto-width></uui-input>
				</div>
			</div>
			<div slot="header-actions">
				${when(
					this._hasOwnerContainer === false && this._inheritedFrom,
					() => html`
						<uui-tag look="default" class="inherited">
							<uui-icon name="icon-merge"></uui-icon>
							<span
								>${this.localize.term('contentTypeEditor_inheritedFrom')}
								${repeat(
									this._inheritedFrom!,
									(inherited) => inherited.unique,
									(inherited) => html`
										<a href=${this.editContentTypePath + 'edit/' + inherited.unique}>${inherited.name}</a>
									`,
								)}
							</span>
						</uui-tag>
					`,
				)}
				${when(
					!this._inherited && !this.sortModeActive,
					() => html`
						<uui-button compact label=${this.localize.term('actions_delete')} @click=${this.#requestRemove}>
							<uui-icon name="delete"></uui-icon>
						</uui-button>
					`,
				)}
				${when(
					this.sortModeActive,
					() => html`
						<uui-input
							type="number"
							label=${this.localize.term('sort_sortOrder')}
							.value=${this.group!.sortOrder.toString()}
							?disabled=${!this._hasOwnerContainer}
							@change=${(e: UUIInputEvent) =>
								this._singleValueUpdate('sortOrder', parseInt(e.target.value as string) ?? 0)}></uui-input>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host([drag-placeholder]) {
				opacity: 0.5;
			}

			:host([drag-placeholder]) > uui-box {
				--uui-box-default-padding: 0;
			}
			:host([drag-placeholder]) umb-content-type-design-editor-properties {
				visibility: hidden;
				position: absolute;
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
				width: 100%;
			}

			#group-name {
				--uui-input-border-color: transparent;
				min-width: 200px;
			}

			uui-input[type='number'] {
				max-width: 75px;
			}

			.inherited uui-icon {
				vertical-align: sub;
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
