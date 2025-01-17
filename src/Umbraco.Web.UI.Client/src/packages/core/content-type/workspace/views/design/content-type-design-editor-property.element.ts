import type { UmbContentTypePropertyStructureHelper } from '../../../structure/index.js';
import type { UmbContentTypeModel, UmbPropertyTypeModel, UmbPropertyTypeScaffoldModel } from '../../../types.js';
import { UmbPropertyTypeContext } from './content-type-design-editor-property.context.js';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_EDIT_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/property-type';
import type { UUIInputElement, UUIInputLockElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 *  @element umb-content-type-design-editor-property
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 */
@customElement('umb-content-type-design-editor-property')
export class UmbContentTypeDesignEditorPropertyElement extends UmbLitElement {
	#context = new UmbPropertyTypeContext(this);
	#dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
	#dataTypeUnique?: string;
	#propertyUnique?: string;

	@property({ attribute: false })
	public set propertyStructureHelper(value: UmbContentTypePropertyStructureHelper<UmbContentTypeModel> | undefined) {
		if (value === this._propertyStructureHelper) return;
		this._propertyStructureHelper = value;
		this.#checkInherited();
	}
	public get propertyStructureHelper(): UmbContentTypePropertyStructureHelper<UmbContentTypeModel> | undefined {
		return this._propertyStructureHelper;
	}
	private _propertyStructureHelper?: UmbContentTypePropertyStructureHelper<UmbContentTypeModel> | undefined;

	/**
	 * Property, the data object for the property.
	 * @type {UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel | undefined}
	 * @attr
	 * @default
	 */
	@property({ type: Object })
	public get property(): UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel | undefined {
		return this._property;
	}
	public set property(value: UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel | undefined) {
		const oldValue = this._property;
		if (value === oldValue) return;
		this._property = value;
		this.#context.setAlias(value?.alias);
		this.#context.setLabel(value?.name);
		this.#checkAliasAutoGenerate(this._property?.id);
		this.#checkInherited();
		this.#setDataType(this._property?.dataType?.unique);
		this.requestUpdate('property', oldValue);
	}
	private _property?: UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel | undefined;

	@property({ type: Boolean, reflect: true, attribute: 'sort-mode-active' })
	public sortModeActive = false;

	@property({ attribute: false })
	public editContentTypePath?: string;

	@property({ type: Boolean, reflect: true, attribute: '_inherited' })
	public _inherited?: boolean;

	@state()
	public _inheritedContentTypeId?: string;

	@state()
	public _inheritedContentTypeName?: string;

	@property({ type: String, reflect: false })
	protected editPropertyTypePath?: string;

	@state()
	private _dataTypeName?: string;

	@state()
	private _aliasLocked = true;

	#autoGenerateAlias = true;

	#checkAliasAutoGenerate(unique: string | undefined) {
		if (unique === this.#propertyUnique) return;
		this.#propertyUnique = unique;

		if (this.#context.getAlias()) {
			this.#autoGenerateAlias = false;
		}
	}

	async #checkInherited() {
		if (this._propertyStructureHelper && this._property) {
			// We can first match with something if we have a name [NL]
			this.observe(
				await this._propertyStructureHelper!.contentTypeOfProperty(this._property.id),
				(contentType) => {
					this._inherited =
						this._propertyStructureHelper?.getStructureManager()?.getOwnerContentTypeUnique() !== contentType?.unique;
					this._inheritedContentTypeId = contentType?.unique;
					this._inheritedContentTypeName = contentType?.name;
				},
				'observeIsOwnerProperty',
			);
		}
	}

	#partialUpdate(partialObject: UmbPropertyTypeModel) {
		if (!this._property || !this._propertyStructureHelper) return;
		this._propertyStructureHelper.partialUpdateProperty(this._property.id, partialObject);
	}

	#singleValueUpdate<PropertyNameType extends keyof UmbPropertyTypeModel>(
		propertyName: PropertyNameType,
		value: UmbPropertyTypeModel[PropertyNameType],
	) {
		if (!this._property || !this._propertyStructureHelper) return;
		const partialObject: Partial<UmbPropertyTypeModel> = {};
		partialObject[propertyName] = value === null ? undefined : value;
		this._propertyStructureHelper.partialUpdateProperty(this._property.id, partialObject);
	}

	#onToggleAliasLock(event: CustomEvent) {
		if (!this.property?.alias && (event.target as UUIInputLockElement).locked) {
			this.#autoGenerateAlias = true;
		} else {
			this.#autoGenerateAlias = false;
		}

		this._aliasLocked = !this._aliasLocked;
		if (!this._aliasLocked) {
			(event.target as UUIInputElement)?.focus();
		}
	}

	async #setDataType(dataTypeUnique: string | undefined) {
		if (!dataTypeUnique) {
			this._dataTypeName = undefined;
			this.#dataTypeUnique = undefined;
			return;
		}
		if (dataTypeUnique === this.#dataTypeUnique) return;
		this.#dataTypeUnique = dataTypeUnique;
		this.#dataTypeDetailRepository.requestByUnique(dataTypeUnique).then((x) => (this._dataTypeName = x?.data?.name));
	}

	async #requestRemove(e: Event) {
		e.preventDefault();
		e.stopImmediatePropagation();
		if (!this._property || !this._property.id) return;

		// TODO: Do proper localization here: [NL]
		await umbConfirmModal(this, {
			headline: `${this.localize.term('actions_delete')} property`,
			content: html`<umb-localize key="contentTypeEditor_confirmDeletePropertyMessage" .args=${[this._property.name ?? this._property.id]}>Are you sure you want to delete the property <strong>${this._property.name ?? this._property.id}</strong></umb-localize></div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		});

		this._propertyStructureHelper?.removeProperty(this._property.id);
	}

	#onAliasChanged(event: UUIInputEvent) {
		this.#singleValueUpdate('alias', event.target.value.toString());
	}

	#onNameChanged(event: UUIInputEvent) {
		const newName = event.target.value.toString();
		if (this.#autoGenerateAlias) {
			this.#singleValueUpdate('alias', generateAlias(newName ?? ''));
		}
		this.#singleValueUpdate('name', newName);
	}

	override render() {
		// TODO: Only show alias on label if user has access to DocumentType within settings: [NL]
		return this._inherited ? this.renderInheritedProperty() : this.renderEditableProperty();
	}

	renderInheritedProperty() {
		if (!this.property) return;

		if (this.sortModeActive) {
			return this.renderSortableProperty();
		} else {
			return html`
				<div id="header">
					<b>${this.property.name}</b>
					<i>${this.property.alias}</i>
					<p>${this.property.description}</p>
				</div>
				<div id="editor">
					${this.renderPropertyTags()}
					${this._inherited
						? html`<uui-tag look="default" class="inherited">
								<uui-icon name="icon-merge"></uui-icon>
								<span
									>${this.localize.term('contentTypeEditor_inheritedFrom')}
									<a href=${this.editContentTypePath + 'edit/' + this._inheritedContentTypeId}>
										${this._inheritedContentTypeName ?? '??'}
									</a>
								</span>
							</uui-tag>`
						: nothing}
				</div>
			`;
		}
	}

	renderEditableProperty() {
		if (!this.property || !this.editPropertyTypePath) return;

		if (this.sortModeActive) {
			return this.renderSortableProperty();
		} else {
			return html`
				<div id="header">
					<uui-input
						name="label"
						id="label-input"
						placeholder=${this.localize.term('placeholders_label')}
						label="label"
						.value=${this.property.name}
						@input=${this.#onNameChanged}></uui-input>
					${this.renderPropertyAlias()}
					<slot name="action-menu"></slot>
					<p>
						<uui-textarea
							label="description"
							name="description"
							id="description-input"
							placeholder=${this.localize.term('placeholders_enterDescription')}
							.value=${this.property.description}
							@input=${(e: CustomEvent) => {
								if (e.target) this.#singleValueUpdate('description', (e.target as HTMLInputElement).value);
							}}
							auto-height></uui-textarea>
					</p>
				</div>
				<uui-button
					id="editor"
					look="secondary"
					label=${this.localize.term('contentTypeEditor_editorSettings')}
					href=${this.editPropertyTypePath +
					UMB_EDIT_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this.property.id })}>
					${this.renderPropertyTags()}
					<uui-action-bar>
						<uui-button label="${this.localize.term('actions_delete')}" @click="${this.#requestRemove}">
							<uui-icon name="delete"></uui-icon>
						</uui-button>
					</uui-action-bar>
				</uui-button>
			`;
		}
	}

	renderSortableProperty() {
		if (!this.property) return;
		return html`
			<div class="sortable">
				<uui-icon name="icon-navigation"></uui-icon>
				<span>${this.property.name}</span>
				<span style="color: var(--uui-color-disabled-contrast)">(${this.property.alias})</span>
			</div>
			<uui-input
				type="number"
				?disabled=${this._inherited}
				label="sort order"
				@change=${(e: UUIInputEvent) =>
					this.#partialUpdate({ sortOrder: parseInt(e.target.value as string) ?? 0 } as UmbPropertyTypeModel)}
				.value=${this.property.sortOrder ?? 0}></uui-input>
		`;
	}

	renderPropertyAlias() {
		if (!this.property) return;
		return html`
			<uui-input-lock
				name="alias"
				id="alias-input"
				label=${this.localize.term('placeholders_enterAlias')}
				placeholder=${this.localize.term('placeholders_enterAlias')}
				.value=${this.property.alias}
				?locked=${this._aliasLocked}
				@input=${this.#onAliasChanged}
				@lock-change=${this.#onToggleAliasLock}>
			</uui-input-lock>
		`;
	}

	renderPropertyTags() {
		return this.property
			? html`<div class="types">
					${this.property.dataType?.unique ? html`<uui-tag look="default">${this._dataTypeName}</uui-tag>` : nothing}
					${this.property.variesByCulture
						? html`<uui-tag look="default">
								<uui-icon name="icon-shuffle"></uui-icon> ${this.localize.term('contentTypeEditor_cultureVariantLabel')}
							</uui-tag>`
						: nothing}
					${this.property.appearance?.labelOnTop == true
						? html`<uui-tag look="default">
								<span>${this.localize.term('contentTypeEditor_displaySettingsLabelOnTop')}</span>
							</uui-tag>`
						: nothing}
					${this.property.validation.mandatory === true
						? html`<uui-tag look="default">
								<span>* ${this.localize.term('general_mandatory')}</span>
							</uui-tag>`
						: nothing}
					${this.property.visibility?.memberCanView === true
						? html`<uui-tag look="default">
								<uui-icon name="icon-eye"></uui-icon> ${this.localize.term('contentTypeEditor_showOnMemberProfile')}
							</uui-tag>`
						: nothing}
					${this.property.visibility?.memberCanEdit === true
						? html`<uui-tag look="default">
								<uui-icon name="icon-edit"></uui-icon> ${this.localize.term('contentTypeEditor_memberCanEdit')}
							</uui-tag>`
						: nothing}
					${this.property.isSensitive === true
						? html`<uui-tag look="default">
								<uui-icon name="icon-lock"></uui-icon> ${this.localize.term('contentTypeEditor_isSensitiveData')}
							</uui-tag>`
						: nothing}
				</div>`
			: nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host(:not([sort-mode-active])) {
				display: grid;
				grid-template-columns: 200px auto;
				column-gap: var(--uui-size-layout-2);
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-layout-1) 0;
				container-type: inline-size;
			}

			:host > div {
				grid-column: span 2;
			}

			@container (width > 600px) {
				:host(:not([orientation='vertical'])) > div {
					grid-column: span 1;
				}
			}

			:host(:first-of-type) {
				padding-top: 0;
			}
			:host(:last-of-type) {
				border-bottom: none;
			}

			:host([sort-mode-active]) {
				position: relative;
				display: flex;
				padding: 0;
				margin-bottom: var(--uui-size-3);
			}

			:host([sort-mode-active]:last-of-type) {
				margin-bottom: 0;
			}

			:host([sort-mode-active]:not([_inherited])) {
				cursor: grab;
			}

			:host([sort-mode-active]) .sortable {
				flex: 1;
				display: flex;
				align-items: center;
				padding: 0 var(--uui-size-3);
				gap: var(--uui-size-3);
			}
			:host([sort-mode-active][_inherited]) .sortable {
				color: var(--uui-color-disabled-contrast);
			}
			:host([sort-mode-active]:not([_inherited])) .sortable {
				background-color: var(--uui-color-divider);
			}

			:host([sort-mode-active]) uui-input {
				max-width: 75px;
			}

			/* Placeholder style, used when property is being dragged.*/
			:host(.--umb-sorter-placeholder) > * {
				visibility: hidden;
			}

			:host(.--umb-sorter-placeholder)::after {
				content: '';
				inset: 0;
				position: absolute;
				border: 1px dashed var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
			}

			p {
				margin-bottom: 0;
			}

			#header {
				position: sticky;
				top: var(--uui-size-space-4);
				height: min-content;
			}

			#editor {
				position: relative;
			}
			#alias-input,
			#label-input,
			#description-input {
				width: 100%;
			}

			#alias-input {
				border-color: transparent;
				background: var(--uui-color-surface);
			}

			#label-input {
				font-weight: bold; /* TODO: UUI Input does not support bold text yet */
				--uui-input-border-color: transparent;
			}
			#label-input input {
				font-weight: bold;
				--uui-input-border-color: transparent;
			}

			#description-input {
				--uui-textarea-border-color: transparent;
				font-weight: 0.5rem; /* TODO: Cant change font size of UUI textarea yet */
			}

			.types > div uui-icon,
			.inherited uui-icon {
				vertical-align: sub;
				margin-right: var(--uui-size-space-1);
			}

			.inherited {
				position: absolute;
				top: var(--uui-size-space-2);
				right: var(--uui-size-space-2);
			}

			.types {
				position: absolute;
				top: var(--uui-size-space-2);
				left: var(--uui-size-space-2);
				display: flex;
				flex-flow: wrap;
				gap: var(--uui-size-space-2);
			}

			#editor uui-action-bar {
				position: absolute;
				top: var(--uui-size-space-2);
				right: var(--uui-size-space-2);
				opacity: 0;
			}

			#editor:hover uui-action-bar,
			#editor:focus uui-action-bar,
			#editor:focus-within uui-action-bar {
				opacity: 1;
			}

			a {
				color: inherit;
			}

			:host([drag-placeholder]) {
				opacity: 0.5;
			}
			:host([drag-placeholder]) uui-input {
				visibility: hidden;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-design-editor-property': UmbContentTypeDesignEditorPropertyElement;
	}
}
