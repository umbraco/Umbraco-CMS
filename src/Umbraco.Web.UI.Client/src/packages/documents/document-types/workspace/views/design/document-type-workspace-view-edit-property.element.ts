import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { PropertyTypeResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_PROPERTY_SETTINGS_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 *  @element document-type-workspace-view-edit-property
 *  @description - Element for displaying a property in an workspace.
 *  @slot editor - Slot for rendering the Property Editor
 */
@customElement('document-type-workspace-view-edit-property')
export class UmbDocumentTypeWorkspacePropertyElement extends UmbLitElement {
	private _property?: PropertyTypeResponseModelBaseModel | undefined;
	/**
	 * Property, the data object for the property.
	 * @type {PropertyTypeResponseModelBaseModel}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Object })
	public get property(): PropertyTypeResponseModelBaseModel | undefined {
		return this._property;
	}
	public set property(value: PropertyTypeResponseModelBaseModel | undefined) {
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

	_partialUpdate(partialObject: PropertyTypeResponseModelBaseModel) {
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

	renderEditableProperty() {
		return this.property
			? html`
					<div id="header">
						<uui-input
							.value=${this.property.name}
							@input=${(e: CustomEvent) => {
								if (e.target) this._singleValueUpdate('name', (e.target as HTMLInputElement).value);
							}}></uui-input>
						<uui-input-lock
							.value=${this.property.alias}
							@input=${(e: CustomEvent) => {
								if (e.target) this._singleValueUpdate('alias', (e.target as HTMLInputElement).value);
							}}></uui-input-lock>
						<slot name="property-action-menu"></slot>
						<p>
							<uui-textarea
								.value=${this.property.description}
								@input=${(e: CustomEvent) => {
									if (e.target) this._singleValueUpdate('description', (e.target as HTMLInputElement).value);
								}}></uui-textarea>
						</p>
					</div>
					<uui-button id="editor" label="Edit property settings" href=${this._modalRoute}><b></b></uui-button>
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'document-type-workspace-view-edit-property': UmbDocumentTypeWorkspacePropertyElement;
	}
}
