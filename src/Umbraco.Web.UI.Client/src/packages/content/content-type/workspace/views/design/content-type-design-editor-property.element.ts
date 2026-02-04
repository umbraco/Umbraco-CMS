import type { UmbContentTypePropertyStructureHelper } from '../../../structure/index.js';
import type { UmbContentTypeModel, UmbPropertyTypeModel, UmbPropertyTypeScaffoldModel } from '../../../types.js';
import { UmbPropertyTypeContext } from './content-type-design-editor-property.context.js';
import { css, html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_EDIT_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/property-type';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';

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
		this.#checkInherited();
		this.#setDataType(this._property?.dataType?.unique);
		this.requestUpdate('property', oldValue);
	}
	private _property?: UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel | undefined;

	@property({ type: Boolean, reflect: true, attribute: 'sort-mode-active' })
	public sortModeActive = false;

	@property({ attribute: false })
	public editContentTypePath?: string;

	@property({ attribute: false })
	public ownerVariesByCulture?: boolean;

	@property({ attribute: false })
	public ownerVariesBySegment?: boolean;

	@property({ type: Boolean, reflect: true, attribute: '_inherited' })
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _inherited?: boolean;

	@state()
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _inheritedContentTypeId?: string;

	@state()
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _inheritedContentTypeName?: string;

	@property({ type: String, reflect: false })
	protected editPropertyTypePath?: string;

	@state()
	private _dataTypeName?: string;

	async #checkInherited() {
		if (this._propertyStructureHelper && this._property) {
			// We can first match with something if we have a name [NL]
			this.observe(
				await this._propertyStructureHelper!.contentTypeOfProperty(this._property.unique),
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
		this._propertyStructureHelper.partialUpdateProperty(this._property.unique, partialObject);
	}

	#singleValueUpdate<PropertyNameType extends keyof UmbPropertyTypeModel>(
		propertyName: PropertyNameType,
		value: UmbPropertyTypeModel[PropertyNameType],
	) {
		if (!this._property || !this._propertyStructureHelper) return;
		const partialObject: Partial<UmbPropertyTypeModel> = {};
		partialObject[propertyName] = value === null ? undefined : value;
		this._propertyStructureHelper.partialUpdateProperty(this._property.unique, partialObject);
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
		if (!this._property || !this._property.unique) return;

		const unique = this._property.unique;

		// TODO: Do proper localization here: [NL]
		await umbConfirmModal(this, {
			headline: `${this.localize.term('actions_delete')} property`,
			content: html`<umb-localize key="contentTypeEditor_confirmDeletePropertyMessage" .args=${[this._property.name ?? unique]}>Are you sure you want to delete the property <strong>${this._property.name ?? unique}</strong></umb-localize></div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		});

		this._propertyStructureHelper?.removeProperty(unique);
	}

	#onNameAliasChange(e: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#partialUpdate({
			name: e.target.value,
			alias: e.target.alias,
		} as UmbPropertyTypeModel);
	}

	override render() {
		// TODO: Only show alias on label if user has access to DocumentType within settings: [NL]
		return this._inherited ? this.#renderInheritedProperty() : this.#renderEditableProperty();
	}

	#renderInheritedProperty() {
		if (!this.property) return;

		if (this.sortModeActive) {
			return this.#renderSortableProperty();
		} else {
			return html`
				<div id="header">
					<p>${this.localize.string(this.property.name)}<i>${this.property.alias}</i></p>
					<p>${this.property.description}</p>
				</div>
				<div id="editor">
					${this.#renderPropertyName()} ${this.#renderPropertyTags()}
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

	#renderEditableProperty() {
		if (!this.property || !this.editPropertyTypePath) return;

		if (this.sortModeActive) {
			return this.#renderSortableProperty();
		} else {
			return html`
				<div id="header">
					<umb-input-with-alias
						name="name"
						id="name-alias-input"
						required
						.placeholder=${this.localize.term('placeholders_label')}
						.label=${this.localize.term('placeholders_label')}
						.aliasLabel=${this.localize.term('placeholders_enterAlias')}
						.value=${this.property.name}
						.alias=${this.property.alias}
						@change=${this.#onNameAliasChange}
						${umbBindToValidation(this)}></umb-input-with-alias>
					<umb-form-validation-message for="name-alias-input"></umb-form-validation-message>

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
					look="outline"
					label=${this.localize.term('contentTypeEditor_editorSettings')}
					href=${this.editPropertyTypePath +
					UMB_EDIT_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({ unique: this.property.unique })}>
					${this.#renderPropertyName()} ${this.#renderPropertyTags()}
					<uui-action-bar>
						<uui-button label="${this.localize.term('actions_delete')}" @click="${this.#requestRemove}">
							<uui-icon name="delete"></uui-icon>
						</uui-button>
					</uui-action-bar>
				</uui-button>
			`;
		}
	}

	#onPropertyOrderChanged = (e: UUIInputEvent) =>
		this.#partialUpdate({ sortOrder: parseInt(e.target.value as string) ?? 0 } as UmbPropertyTypeModel);

	#renderSortableProperty() {
		if (!this.property) return;
		return html`
			<div class="sortable">
				<uui-icon name="icon-grip"></uui-icon>
				<span>${this.property.name}</span>
				<span style="color: var(--uui-color-disabled-contrast)">(${this.property.alias})</span>
			</div>
			<uui-input
				type="number"
				?disabled=${this._inherited}
				label="sort order"
				@change=${this.#onPropertyOrderChanged}
				.value=${(this.property.sortOrder ?? 0).toString()}></uui-input>
		`;
	}

	#renderPropertyName() {
		return this.property?.dataType?.unique ? html`<div id="editor-name">${this._dataTypeName}</div>` : nothing;
	}

	#renderPropertyTags() {
		return this.property
			? html`<div class="types">
					${this.#renderVariantTags()}
					${this.property.appearance?.labelOnTop == true
						? html`<uui-tag look="default">
								<uui-icon name="icon-stretch-horizontal"></uui-icon>
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

	#renderVariantTags() {
		if (!this.property) return nothing;

		if (
			this.ownerVariesByCulture &&
			this.ownerVariesBySegment &&
			!this.property.variesByCulture &&
			!this.property.variesBySegment
		) {
			return html`
				<uui-tag look="default">
					<uui-icon name="icon-trending-up-down"></uui-icon> ${this.localize.term(
						'contentTypeEditor_cultureAndVariantInvariantLabel',
					)}
				</uui-tag>
			`;
		}

		if (this.ownerVariesByCulture && !this.property.variesByCulture) {
			return html`<uui-tag look="default">
				<uui-icon name="icon-trending-up-down"></uui-icon> ${this.localize.term(
					'contentTypeEditor_cultureInvariantLabel',
				)}
			</uui-tag>`;
		}

		if (this.ownerVariesBySegment && !this.property.variesBySegment) {
			return html`<uui-tag look="default">
				<uui-icon name="icon-trending-up-down"></uui-icon> ${this.localize.term(
					'contentTypeEditor_segmentInvariantLabel',
				)}
			</uui-tag>`;
		}

		// Not shared:
		return nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				border-bottom: 1px solid var(--uui-color-divider);
				padding: var(--uui-size-layout-1) 0;
			}
			:host(:not([sort-mode-active])) {
				display: grid;
				grid-template-columns: 320px auto;
				column-gap: var(--uui-size-space-5);
				container-type: inline-size;
			}

			:host > div {
				grid-column: span 2;
			}

			@container (width > 700px) {
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
			}

			:host([sort-mode-active]:not([_inherited])) {
				cursor: grab;
			}
			:host([sort-mode-active][_inherited]) {
				cursor: not-allowed;
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

			:host([_inherited]) {
				#header {
					padding: 0 var(--uui-size-3, 9px);
				}
			}

			p {
				margin-top: 0;
				margin-bottom: 0;
			}

			#header {
				position: sticky;
				top: var(--uui-size-space-4);
				height: min-content;
			}

			#header i {
				opacity: 0.55;
				float: right;
			}

			#header umb-input-with-alias {
				--uui-input-border-color: transparent;
			}
			#name-alias-input,
			#description-input {
				width: 100%;
			}

			#description-input:not(:hover):not(:focus) {
				--uui-textarea-border-color: transparent;
			}

			#editor {
				position: relative;
				--uui-button-background-color: var(--uui-color-background);
				--uui-button-background-color-hover: var(--uui-color-background);
			}
			#editor:not(uui-button) {
				background-color: var(--uui-color-background);
				border-radius: var(--uui-button-border-radius, var(--uui-border-radius, 3px));
				min-height: 92px;
			}
			#editor uui-action-bar {
				--uui-button-background-color: var(--uui-color-surface);
				--uui-button-background-color-hover: var(--uui-color-surface);
			}
			#editor-name {
				position: absolute;
				top: var(--uui-size-space-3);
				left: var(--uui-size-space-4);
				font-size: var(--uui-type-small-size);
				font-weight: 400;
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
				bottom: var(--uui-size-space-2);
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
