import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_PROPERTY_SETTINGS_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { generateAlias } from '@umbraco-cms/backoffice/utils';

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

	#modalRegistration;

	@state()
	protected _modalRoute?: string;

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
	}

	_partialUpdate(partialObject: PropertyTypeModelBaseModel) {
		this.dispatchEvent(new CustomEvent('partial-property-update', { detail: partialObject }));
	}

	_singleValueUpdate(propertyName: string, value: string | number | boolean | null | undefined) {
		const partialObject = {} as any;
		partialObject[propertyName] = value;

		this.dispatchEvent(new CustomEvent('partial-property-update', { detail: partialObject }));
	}

	renderInheritedProperty() {
		return this.property
			? html`
					<div id="header">
						<b>${this.property.name}</b>
						<i>${this.property.alias}</i>
						<p>${this.property.description}</p>
					</div>
					<div id="editor"></div>
			  `
			: '';
	}

	@state()
	private _aliasLocked = true;

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
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
							placeholder="Label..."
							label="label"
							.value=${this.property.name}
							@input=${this.#onNameChange}></uui-input>
						<!-- TODO: should use UUI-LOCK-INPUT, but that does not fire an event when its locked/unlocked -->
						<uui-input
							name="alias"
							id="alias-input"
							label="alias"
							placeholder="Alias..."
							.value=${this.property.alias}
							?disabled=${this._aliasLocked}
							@input=${(e: CustomEvent) => {
								if (e.target) this._singleValueUpdate('alias', (e.target as HTMLInputElement).value);
							}}>
							<!-- TODO: validation for bad characters -->
							<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
								<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
							</div>
						</uui-input>
						<slot name="property-action-menu"></slot>
						<p>
							<uui-textarea
								label="description"
								name="description"
								id="description-input"
								placeholder="Enter a description..."
								.value=${this.property.description}
								@input=${(e: CustomEvent) => {
									if (e.target) this._singleValueUpdate('description', (e.target as HTMLInputElement).value);
								}}></uui-textarea>
						</p>
					</div>
					<uui-button id="editor" label="Edit property settings" href=${ifDefined(this._modalRoute)}>
						<b></b>
					</uui-button>
			  `
			: '';
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
			:host(.--umb-sorter-placeholder) > div {
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'document-type-workspace-view-edit-property': UmbDocumentTypeWorkspacePropertyElement;
	}
}
