import type { UmbTemplateCardElement } from '../template-card/template-card.element.js';
import '../template-card/template-card.element.js';
import type { UmbTemplateItemModel } from '../../repository/item/index.js';
import { UmbTemplateItemRepository } from '../../repository/item/index.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { FormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import {
	UMB_TEMPLATE_PICKER_MODAL,
	UMB_TEMPLATE_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-template')
export class UmbInputTemplateElement extends FormControlMixin(UmbLitElement) {
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

	_selectedIds: Array<string> = [];
	@property({ type: Array })
	public get selectedIds() {
		return this._selectedIds;
	}
	public set selectedIds(newKeys: Array<string> | undefined) {
		this._selectedIds = newKeys ?? [];
		this.#observePickedTemplates();
	}

	_defaultUnique = '';
	@property({ type: String })
	public get defaultUnique(): string {
		return this._defaultUnique;
	}
	public set defaultUnique(newId: string) {
		this._defaultUnique = newId;
		super.value = newId;
		this.#observePickedTemplates();
	}

	private _modalContext?: UmbModalManagerContext;
	private _templateItemRepository = new UmbTemplateItemRepository(this);

	@state()
	_pickedTemplates: UmbTemplateItemModel[] = [];

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
	}

	async #observePickedTemplates() {
		this.observe(
			(await this._templateItemRepository.requestItems(this._selectedIds)).asObservable(),
			(data) => {
				const oldValue = this._pickedTemplates;
				this._pickedTemplates = data;
				this.requestUpdate('_pickedTemplates', oldValue);
			},
			'_observeTemplates',
		);
	}

	protected getFormElement() {
		return this;
	}

	#onCardChange(e: CustomEvent) {
		e.stopPropagation();
		const unique = (e.target as UmbTemplateCardElement).value as string;
		this.defaultUnique = unique;
		this.dispatchEvent(new CustomEvent('change'));
	}

	#openPicker() {
		// TODO: Change experience, so its not multi selectable. But instead already picked templates should be unpickable. (awaiting general picker features for such)
		const modalContext = this._modalContext?.open(UMB_TEMPLATE_PICKER_MODAL, {
			data: {
				multiple: true,
				pickableFilter: (template) => template.unique !== null,
			},
			value: {
				selection: [...this._selectedIds],
			},
		});

		modalContext?.onSubmit().then((value) => {
			if (!value?.selection) return;
			this.selectedIds = value.selection.filter((x) => x !== null) as Array<string>;
			this.dispatchEvent(new CustomEvent('change'));
		});
	}

	#removeTemplate(unique: string) {
		/*
		TODO: We need to follow up on this experience.
		Could we test if this document type is in use, if so we should have a dialog notifying the user(Dialog, are you sure...) about that we might will break something?
		If thats the case, Im not why if a template will be removed from an actual document.
		If if its just the option that will go away.
		(Comment by Niels)
		In current backoffice we just prevent deleting a default when there are other templates. But if its the only one its okay. This is a weird experience, so we should make something that makes more sense.
		BTW. its weird cause the damage of removing the default template is equally bad when there is one or more templates.
		*/
		this.selectedIds = this._selectedIds.filter((x) => x !== unique);
	}

	#openTemplate(e: CustomEvent) {
		const unique = (e.target as UmbTemplateCardElement).value as string;

		this._modalContext?.open(UMB_TEMPLATE_MODAL, {
			data: {
				unique,
				language: 'razor',
			},
		});
	}

	render() {
		return html`
			${this._pickedTemplates.map(
				(template) => html`
					<umb-template-card
						.name="${template.name ?? ''}"
						.id="${template.unique ?? ''}"
						@change=${this.#onCardChange}
						@open="${this.#openTemplate}"
						?default="${template.unique === this.defaultUnique}">
						<uui-button
							slot="actions"
							label="Remove Template ${template.name}"
							@click="${() => this.#removeTemplate(template.unique ?? '')}"
							compact>
							<uui-icon name="icon-trash"></uui-icon>
						</uui-button>
					</umb-template-card>
				`,
			)}
			<uui-button id="add-button" look="placeholder" label="open" @click="${this.#openPicker}">Add</uui-button>
		`;
	}

	static styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
				grid-template-rows: repeat(auto-fill, minmax(160px, 1fr));
			}

			#add-button {
				text-align: center;
				height: 100%;
			}
		`,
	];
}

export default UmbInputTemplateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-template': UmbInputTemplateElement;
	}
}
