import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { TemplateResource, TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTemplateCardElement } from '../template-card/template-card.element';
import { UMB_TEMPLATE_PICKER_MODAL_TOKEN } from '../../modals/template-picker';
import { UMB_TEMPLATE_MODAL_TOKEN } from '../../modals/template';

@customElement('umb-input-template-picker')
export class UmbInputTemplatePickerElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			#add-button {
				width: 100%;
			}
			:host {
				box-sizing: border-box;
				display: flex;
				gap: var(--uui-size-space-4);
				flex-wrap: wrap;
			}

			:host > * {
				max-width: 180px;
				min-width: 180px;
				min-height: 150px;
			}
		`,
	];
	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	_allowedKeys: Array<string> = [];
	@property({ type: Array<string> })
	public get allowedKeys() {
		return this._allowedKeys;
	}
	public set allowedKeys(newKeys: Array<string>) {
		//this.#observePickedTemplates();
		this._allowedKeys = newKeys;
		//this._templates = [];
		//this._allowedKeys.forEach((key) => {
		//	this.#setup(key);
		//});
	}

	_defaultKey = '';
	@property({ type: String })
	public get defaultKey(): string {
		return this._defaultKey;
	}
	public set defaultKey(newKey: string) {
		this._defaultKey = newKey;
		super.value = newKey;
	}

	private _modalContext?: UmbModalContext;
	//private _templateStore?: UmbTemplateTreeStore;
	//private _pickedItemsObserver?: UmbObserverController<EntityTreeItemResponseModel[]>;

	@state()
	_templates: TemplateResponseModel[] = [];

	public get templates(): TemplateResponseModel[] {
		return this._templates;
	}
	public set templates(newTemplates: TemplateResponseModel[]) {
		this._templates = newTemplates;
		this.allowedKeys = newTemplates.map((template) => template.key ?? '');
	}

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.allowedKeys.forEach((key) => this.#setup(key));
	}

	async #setup(templateKey: string) {
		const { data } = await tryExecuteAndNotify(this, TemplateResource.getTemplateByKey({ key: templateKey }));
		if (!data) return;
		this.templates = [...this.templates, data];
	}

	protected getFormElement() {
		return undefined;
	}

	#changeDefault(e: CustomEvent) {
		e.stopPropagation();
		const newKey = (e.target as UmbTemplateCardElement).value as string;
		this.defaultKey = newKey;
		this.dispatchEvent(new CustomEvent('change-default', { bubbles: true, composed: true }));
	}

	#openPicker() {
		const modalHandler = this._modalContext?.open(UMB_TEMPLATE_PICKER_MODAL_TOKEN, {
			multiple: true,
			selection: [...this.allowedKeys],
		});

		modalHandler?.onSubmit().then((data) => {
			if (!data.selection) return;
			//TODO: a better way to do this?
			this.templates = [];
			this.allowedKeys = data.selection;
			this.allowedKeys.forEach((key) => this.#setup(key));
			this.dispatchEvent(new CustomEvent('change-allowed', { bubbles: true, composed: true }));
		});
	}

	#removeTemplate(key: string) {
		console.log('picker: remove', key);
		const templateIndex = this.templates.findIndex((x) => x.key === key);
		this.templates.splice(templateIndex, 1);
		this.templates = [...this._templates];
	}

	render() {
		return html`
			${this.templates.map(
				(template) => html`
					<umb-template-card
						class="template-card"
						.name="${template.name ?? ''}"
						.key="${template.key ?? ''}"
						@change-default="${this.#changeDefault}"
						@open="${this.#openTemplate}"
						?default="${template.key === this.defaultKey}">
						<uui-button
							slot="actions"
							label="Remove document ${template.name}"
							@click="${() => this.#removeTemplate(template.key ?? '')}"
							compact>
							<uui-icon name="umb:trash"> </uui-icon>
						</uui-button>
					</umb-template-card>
				`
			)}
			<uui-button id="add-button" look="placeholder" label="open" @click="${this.#openPicker}">Add</uui-button>
		`;
	}

	#openTemplate(e: CustomEvent) {
		const key = (e.target as UmbTemplateCardElement).value;

		const modalHandler = this._modalContext?.open(UMB_TEMPLATE_MODAL_TOKEN, {
			key: key as string,
			language: 'razor',
		});

		modalHandler?.onSubmit().then(({ key }) => {
			// TODO: update template
		});
	}
}

export default UmbInputTemplatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-template-picker': UmbInputTemplatePickerElement;
	}
}
