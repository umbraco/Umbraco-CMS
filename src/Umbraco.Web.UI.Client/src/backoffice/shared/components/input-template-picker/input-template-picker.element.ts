import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbTemplateCardElement } from '../template-card/template-card.element';
import { UmbTemplateRepository } from '../../../templating/templates/repository/template.repository';
import {
	UMB_TEMPLATE_PICKER_MODAL,
	UMB_TEMPLATE_MODAL,
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { TemplateResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-input-template-picker')
export class UmbInputTemplatePickerElement extends FormControlMixin(UmbLitElement) {
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
		this._allowedKeys = newKeys;
		this.#observePickedTemplates();
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
	private _templateRepository: UmbTemplateRepository = new UmbTemplateRepository(this);

	@state()
	_pickedTemplates: TemplateResponseModel[] = [];

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	async #observePickedTemplates() {
		this.observe(
			await this._templateRepository.treeItems(this._allowedKeys),
			(data) => {
				this._pickedTemplates = data;
			},
			'_templateRepositoryTreeItems'
		);
	}

	protected getFormElement() {
		return this;
	}

	#changeDefault(e: CustomEvent) {
		e.stopPropagation();
		const newKey = (e.target as UmbTemplateCardElement).value as string;
		this.defaultKey = newKey;
		this.dispatchEvent(new CustomEvent('change-default'));
	}

	#openPicker() {
		const modalHandler = this._modalContext?.open(UMB_TEMPLATE_PICKER_MODAL, {
			multiple: true,
			selection: [...this.allowedKeys],
		});

		modalHandler?.onSubmit().then((data) => {
			if (!data.selection) return;
			this.allowedKeys = data.selection;
			this.dispatchEvent(new CustomEvent('change-allowed'));
		});
	}

	#removeTemplate(key: string) {
		/*
		TODO: We need to follow up on this experience.
		Could we test if this document type is in use, if so we should have a dialog notifying the user(Dialog, are you sure...) about that we might will break something?
		If thats the case, Im not why if a template will be removed from an actual document.
		If if its just the option that will go away.
		(Comment by Niels)
		In current backoffice we just prevent deleting a default when there are other templates. But if its the only one its okay. This is a weird experience, so we should make something that makes more sense.
		BTW. its weird cause the damage of removing the default template is equally bad when there is one or more templates.
		*/
		this.allowedKeys = this.allowedKeys.filter((x) => x !== key);
	}

	#openTemplate(e: CustomEvent) {
		const key = (e.target as UmbTemplateCardElement).value;

		this._modalContext?.open(UMB_TEMPLATE_MODAL, {
			key: key as string,
			language: 'razor',
		});
	}

	render() {
		return html`
			${this._pickedTemplates.map(
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
}

export default UmbInputTemplatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-template-picker': UmbInputTemplatePickerElement;
	}
}
