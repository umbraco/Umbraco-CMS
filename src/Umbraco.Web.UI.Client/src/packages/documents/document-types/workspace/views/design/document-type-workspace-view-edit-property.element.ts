import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UMB_CONFIRM_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_PROPERTY_SETTINGS_MODAL,
	UMB_WORKSPACE_MODAL,
	UmbConfirmModalData,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeRepository } from '@umbraco-cms/backoffice/data-type';

/**
 *  @element document-type-workspace-view-edit-property
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 */
@customElement('document-type-workspace-view-edit-property')
export class UmbDocumentTypeWorkspacePropertyElement extends UmbLitElement {
	private _property?: PropertyTypeModelBaseModel | undefined;
	/**
	 * Property, the data object for the property.
	 * @type {PropertyTypeModelBaseModel}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Object })
	public get property(): PropertyTypeModelBaseModel | undefined {
		return this._property;
	}
	public set property(value: PropertyTypeModelBaseModel | undefined) {
		const oldValue = this._property;
		this._property = value;
		this.#modalRegistration.setUniquePathValue('propertyId', value?.id?.toString());
		this.setDataType(this._property?.dataTypeId);
		this.requestUpdate('property', oldValue);
	}

	/**
	 * Inherited, Determines if the property is part of the main document type thats being edited.
	 * If true, then the property is inherited from another document type, not a part of the main document type.
	 * @type {boolean}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Boolean })
	public inherited?: boolean;

	#dataTypeRepository = new UmbDataTypeRepository(this);

	#modalRegistration;
	private _modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT_TOKEN.TYPE;

	@state()
	protected _modalRoute?: string;

	@state()
	protected _editDocumentTypePath?: string;

	@property()
	public get modalRoute() {
		return this._modalRoute;
	}

	@property({ type: String, attribute: 'owner-document-type-id' })
	public ownerDocumentTypeId?: string;

	@property({ type: String, attribute: 'owner-document-type-name' })
	public ownerDocumentTypeName?: string;

	@state()
	private _dataTypeName?: string;

	async setDataType(dataTypeId: string | undefined) {
		if (!dataTypeId) return;
		this.#dataTypeRepository.requestById(dataTypeId).then((x) => (this._dataTypeName = x?.data?.name));
	}

	constructor() {
		super();

		this.#modalRegistration = new UmbModalRouteRegistrationController(this, UMB_PROPERTY_SETTINGS_MODAL)
			.addUniquePaths(['propertyId'])
			.onSetup(() => {
				return this.property ?? false;
			})
			.onSubmit((result) => {
				this._partialUpdate(result);
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder(null);
			});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document-type')
			.onSetup(() => {
				return { entityType: 'document-type', preset: {} };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editDocumentTypePath = routeBuilder({});
			});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (context) => {
			this._modalManagerContext = context;
		});
	}

	_partialUpdate(partialObject: PropertyTypeModelBaseModel) {
		this.dispatchEvent(new CustomEvent('partial-property-update', { detail: partialObject }));
	}

	_singleValueUpdate(propertyName: string, value: string | number | boolean | null | undefined) {
		const partialObject = {} as any;
		partialObject[propertyName] = value;

		this.dispatchEvent(new CustomEvent('partial-property-update', { detail: partialObject }));
	}

	@state()
	private _aliasLocked = true;

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	#requestRemove(e: Event) {
		e.preventDefault();
		e.stopImmediatePropagation();
		if (!this.property || !this.property.id) return;

		const Message: UmbConfirmModalData = {
			headline: `${this.localize.term('actions_delete')} property`,
			content: html`<umb-localize key="contentTypeEditor_confirmDeletePropertyMessage" .args=${[
				this.property.name || this.property.id,
			]}>
					Are you sure you want to delete the property <strong>${this.property.name || this.property.id}</strong>
				</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		};

		const modalHandler = this._modalManagerContext?.open(UMB_CONFIRM_MODAL, Message);

		modalHandler
			?.onSubmit()
			.then(() => {
				this.dispatchEvent(new CustomEvent('property-delete'));
			})
			.catch(() => {
				return;
			});
	}

	#onNameChange(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				const oldName = this.property?.name ?? '';
				const oldAlias = this.property?.alias ?? '';
				const newName = event.target.value.toString();
				if (this._aliasLocked) {
					const expectedOldAlias = generateAlias(oldName ?? '');
					// Only update the alias if the alias matches a generated alias of the old name (otherwise the alias is considered one written by the user.)
					if (expectedOldAlias === oldAlias) {
						this._singleValueUpdate('alias', generateAlias(newName ?? ''));
					}
				}
				this._singleValueUpdate('name', newName);
			}
		}
	}

	renderEditableProperty() {
		return this.property
			? html`
					<div id="header">
						<uui-input
							name="label"
							id="label-input"
							placeholder=${this.localize.term('placeholders_label')}
							label="label"
							.value=${this.property.name}
							@input=${this.#onNameChange}></uui-input>
						${this.renderPropertyAlias()}
						<slot name="property-action-menu"></slot>
						<p>
							<uui-textarea
								label="description"
								name="description"
								id="description-input"
								placeholder=${this.localize.term('placeholders_enterDescription')}
								.value=${this.property.description}
								@input=${(e: CustomEvent) => {
									if (e.target) this._singleValueUpdate('description', (e.target as HTMLInputElement).value);
								}}></uui-textarea>
						</p>
					</div>
					<uui-button
						id="editor"
						label=${this.localize.term('contentTypeEditor_editorSettings')}
						href=${ifDefined(this._modalRoute)}>
						${this.renderPropertyTags()}
						<uui-action-bar>
							<uui-button label="${this.localize.term('actions_delete')}" @click="${this.#requestRemove}">
								<uui-icon name="delete"></uui-icon>
							</uui-button>
						</uui-action-bar>
					</uui-button>
			  `
			: '';
	}

	renderInheritedProperty() {
		return this.property
			? html`
					<div id="header">
						<b>${this.property.name}</b>
						<i>${this.property.alias}</i>
						<p>${this.property.description}</p>
					</div>
					<div id="editor">
						${this.renderPropertyTags()}
						<uui-tag look="default" class="inherited">
							<uui-icon name="umb:merge"></uui-icon>
							<span>
								${this.localize.term('contentTypeEditor_inheritedFrom')}
								<a href=${this._editDocumentTypePath + 'edit/' + this.ownerDocumentTypeId}>
									${this.ownerDocumentTypeName ?? '??'}
								</a>
							</span>
						</uui-tag>
					</div>
			  `
			: '';
	}

	renderPropertyAlias() {
		return this.property
			? html`<uui-input
					name="alias"
					id="alias-input"
					label="alias"
					placeholder=${this.localize.term('placeholders_alias')}
					.value=${this.property.alias}
					?disabled=${this._aliasLocked}
					@input=${(e: CustomEvent) => {
						if (e.target) this._singleValueUpdate('alias', (e.target as HTMLInputElement).value);
					}}>
					<!-- TODO: should use UUI-LOCK-INPUT, but that does not fire an event when its locked/unlocked -->
					<!-- TODO: validation for bad characters -->
					<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
						<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
					</div>
			  </uui-input>`
			: '';
	}

	renderPropertyTags() {
		return this.property
			? html`<div class="types">
					${this.property.dataTypeId ? html`<uui-tag look="default">${this._dataTypeName}</uui-tag>` : nothing}
					${this.property.variesByCulture
						? html`<uui-tag look="default">
								<uui-icon name="umb:shuffle"></uui-icon> ${this.localize.term('contentTypeEditor_cultureVariantLabel')}
						  </uui-tag>`
						: nothing}
					${this.property.appearance?.labelOnTop == true
						? html`<uui-tag look="default">
								<span>${this.localize.term('contentTypeEditor_displaySettingsLabelOnTop')}</span>
						  </uui-tag>`
						: nothing}
			  </div>`
			: nothing;
	}

	render() {
		// TODO: Only show alias on label if user has access to DocumentType within settings:
		return this.inherited ? this.renderInheritedProperty() : this.renderEditableProperty();
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
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

			:host(:last-of-type) {
				border-bottom: none;
			}

			:host(:first-of-type) {
				padding-top: 0;
			}
			:host([draggable='true']) {
				cursor: grab;
			}

			/* Placeholder style, used when property is being dragged.*/
			:host(.--umb-sorter-placeholder) {
				height: 2px;
			}
			:host(.--umb-sorter-placeholder) > div,
			:host(.--umb-sorter-placeholder) > uui-button {
				display: none;
			}
			:host(.--umb-sorter-placeholder)::after {
				content: '';
				grid-column: span 2;
				width: 100%;
				border-top: 2px solid blue;
				border-radius: 1px;
				/* TODO: Make use of same highlight color as UUI and the same Animation. Consider making this a component/(available style) in UUI? */
			}

			p {
				margin-bottom: 0;
			}

			#header {
				position: sticky;
				top: var(--uui-size-space-4);
				height: min-content;
				z-index: 2;
			}

			#editor {
				position: relative;
				background-color: var(--uui-color-background);
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

			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
				/* margin: 0; */
			}
			#description-input {
				--uui-textarea-border-color: transparent;
				font-weight: 0.5rem; /* TODO: Cant change font size of UUI textarea yet */
			}

			.types > div uui-icon,
			.inherited uui-icon {
				vertical-align: sub;
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
				gap: var(--uui-size-space-2);
			}

			#editor uui-action-bar {
				position: absolute;
				top: var(--uui-size-space-2);
				right: var(--uui-size-space-2);
				display: none;
			}
			#editor:hover uui-action-bar,
			#editor:focus uui-action-bar {
				display: block;
			}

			a {
				color: inherit;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'document-type-workspace-view-edit-property': UmbDocumentTypeWorkspacePropertyElement;
	}
}
