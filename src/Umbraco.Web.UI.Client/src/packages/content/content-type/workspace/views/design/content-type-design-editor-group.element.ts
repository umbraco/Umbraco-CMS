import type { UmbContentTypeModel, UmbPropertyTypeContainerMergedModel } from '../../../types.js';
import type { UmbContentTypeContainerStructureHelper } from '../../../structure/index.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UUIBlinkAnimationValue, UUIBlinkKeyframes, type UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

import './content-type-design-editor-properties.element.js';

@customElement('umb-content-type-design-editor-group')
export class UmbContentTypeWorkspaceViewEditGroupElement extends UmbLitElement {
	@property({ attribute: false })
	public set group(value: UmbPropertyTypeContainerMergedModel | undefined) {
		if (value === this._group) return;
		this._group = value;
		this._groupId = value?.ownerId ?? value?.ids[0];
		this.#checkInherited();
	}
	public get group(): UmbPropertyTypeContainerMergedModel | undefined {
		return this._group;
	}
	private _group?: UmbPropertyTypeContainerMergedModel | undefined;

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
	private _groupId?: string;

	@property({ type: Boolean, reflect: true, attribute: 'has-owner-container' })
	private _hasOwnerContainer?: boolean;

	// attrbute is used by Sorter Controller in parent scope.
	@property({ type: Boolean, reflect: true, attribute: 'inherited' })
	private _inherited?: boolean;

	@state()
	private _inheritedFrom?: Array<UmbContentTypeModel>;

	#checkInherited() {
		if (this.groupStructureHelper && this.group) {
			// Check is this container matches with any other group. If so it is inherited aka. merged with others. [NL]
			if (this.group.ownerId) {
				this._hasOwnerContainer = true;
			}

			const notOwnerContainerIds = this.group.ids.filter((id) => id !== this.group!.ownerId);

			if (notOwnerContainerIds.length > 0) {
				this._inheritedFrom = notOwnerContainerIds
					.map((id) => this.groupStructureHelper!.getContentTypeOfContainer(id))
					.filter((contentType) => contentType !== undefined) as Array<UmbContentTypeModel>;
				this._inherited = true;
			} else {
				this._inheritedFrom = undefined;
				this._inherited = false;
			}
		}
	}

	#singleValueUpdate(propertyName: string, value: string | number | boolean | null | undefined) {
		if (!this._groupStructureHelper || !this._group) return;

		const ownerId = this._group.ownerId;
		if (!ownerId) return;

		const partialObject = {} as any;
		partialObject[propertyName] = value;

		this._groupStructureHelper.partialUpdateContainer(ownerId, partialObject);
	}

	#renameGroup(e: InputEvent) {
		if (!this.groupStructureHelper || !this._group) return;
		const ownerId = this._group.ownerId;
		if (!ownerId) return;
		let newName = (e.target as HTMLInputElement).value;
		// TODO: This does not seem right, the detection of a unique name requires better awareness on the level of the change. [NL]
		// This seem to use check for root containers.
		const changedName = this.groupStructureHelper
			.getStructureManager()!
			.makeContainerNameUniqueForOwnerContentType(ownerId, newName);
		if (changedName) {
			newName = changedName;
		}
		this.#singleValueUpdate('name', newName);
		(e.target as HTMLInputElement).value = newName;
	}

	#blurGroup(e: InputEvent) {
		if (!this.groupStructureHelper || !this._group) return;
		const ownerId = this._group.ownerId;
		if (!ownerId) return;
		const newName = (e.target as HTMLInputElement).value;
		if (newName === '') {
			const changedName = this.groupStructureHelper.getStructureManager()!.makeEmptyContainerName(ownerId);
			this.#singleValueUpdate('name', changedName);
			(e.target as HTMLInputElement).value = changedName;
		}
	}

	async #requestRemove(e: Event) {
		e.preventDefault();
		e.stopImmediatePropagation();
		if (!this.groupStructureHelper || !this._group) return;
		if (this._group.ownerId === undefined) return;

		// TODO: Do proper localization here: [NL]
		await umbConfirmModal(this, {
			headline: `${this.localize.term('actions_delete')} group`,
			content: html`<umb-localize key="contentTypeEditor_confirmDeleteGroupMessage" .args=${[
				this._group.name ?? this._group.ownerId,
			]}>
					Are you sure you want to delete the group <strong>${this._group.name ?? this._group.ownerId}</strong>
				</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		});

		this.groupStructureHelper.removeContainer(this._group.ownerId);
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
					${when(this.sortModeActive && this._hasOwnerContainer, () => html`<uui-icon name="icon-grip"></uui-icon>`)}
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
					this._hasOwnerContainer === false && this._inheritedFrom && this._inheritedFrom.length > 0,
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
								this.#singleValueUpdate('sortOrder', parseInt(e.target.value as string) ?? 0)}></uui-input>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		UUIBlinkKeyframes,
		css`
			:host {
				position: relative;
			}

			:host([drag-placeholder]) {
				opacity: 0.5;
			}

			:host::before,
			:host::after {
				content: '';
				position: absolute;
				pointer-events: none;
				inset: 0;
				border-radius: var(--uui-border-radius);
				opacity: 0;
				transition:
					opacity 60ms linear 1ms,
					border-color,
					10ms;
			}

			:host::after {
				display: block;
				z-index: 1;
				border: 2px solid transparent;
			}

			:host([drag-placeholder])::after {
				opacity: 1;
				border-color: var(--uui-color-interactive-emphasis);
				animation: ${UUIBlinkAnimationValue};
			}
			:host([drag-placeholder])::before {
				background-color: var(--uui-color-interactive-emphasis);
				opacity: 0.12;
			}

			:host([drag-placeholder]) uui-box {
				--uui-box-default-padding: 0;
			}
			:host([drag-placeholder]) div[slot='header'],
			:host([drag-placeholder]) div[slot='header-actions'] {
				transition: opacity 60ms linear 1ms;
				opacity: 0;
			}
			:host([drag-placeholder]) umb-content-type-design-editor-properties {
				visibility: hidden;
				display: none;
			}

			uui-box {
				--uui-box-header-padding: 0;
			}

			div[slot='header'] {
				flex: 1;
				display: flex;
				align-items: center;
				justify-content: space-between;
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
			}

			:host([has-owner-container]) div[slot='header'] {
				cursor: grab;
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

			div[slot='header-actions'] {
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
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
