import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, queryAll, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import { TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import { UMB_CONFIRM_MODAL_TOKEN } from '../../modals/confirm';
import { UmbTemplateCardElement } from '../template-card/template-card.element';

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

			.fade-in {
				animation: fadeIn 1s;
			}

			@keyframes fadeIn {
				0% {
					opacity: 0;
				}
				100% {
					opacity: 1;
				}
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

	@state()
	private _items: Array<any> = [
		{ key: '2bf464b6-3aca-4388-b043-4eb439cc2643', name: 'Doc 1', default: false },
		{ key: '9a84c0b3-03b4-4dd4-84ac-706740ac0f71', name: 'Test', default: true },
	];

	private _modalContext?: UmbModalContext;
	//private _documentStore?: UmbDocumentTreeStore;
	//private _pickedItemsObserver?: UmbObserverController<FolderTreeItemModel>;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this._items.length < this.min
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this._items.length > this.max
		);

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._items = this._items.sort((a, b) => b.default - a.default);
		this.#setup();
	}

	async #setup() {
		const templates = await tryExecuteAndNotify(this, TemplateResource.getTreeTemplateRoot({ skip: 0, take: 9999 }));
		console.log(templates);
	}

	protected getFormElement() {
		return undefined;
	}

	#openTemplatePickerModal() {
		console.log('template picker modal');
	}

	#changeSelected() {
		console.log('selected');
	}

	/** Clicking the template card buttons */

	#changeDefault(e: CustomEvent) {
		const key = (e.target as UmbTemplateCardElement).value;

		const oldDefault = this._items.find((x) => x.default === true);
		const newDefault = this._items.find((x) => x.key === key);

		const items = this._items.map((item) => {
			if (item.default === true) return { ...newDefault, default: true };
			if (item.key === key) return { ...oldDefault, default: false };
			return item;
		});

		this._items = items;
	}

	#openTemplate(e: CustomEvent) {
		const key = (e.target as UmbTemplateCardElement).value;
		console.log('open', key);
	}

	#removeTemplate(key: string) {
		console.log('remove', key);
	}

	render() {
		return html`
			${repeat(
				this._items,
				(template) => template.default,
				(template, index) => html`<div class="fade-in">
					<umb-template-card
						class="template-card"
						name="${template.name}"
						key="${template.key}"
						@default-change="${this.#changeDefault}"
						@open="${this.#openTemplate}"
						?default="${template.default}">
						<uui-button
							slot="actions"
							label="Remove document ${template.name}"
							@click="${() => this.#removeTemplate(template.key)}"
							compact>
							<uui-icon name="umb:trash"> </uui-icon>
						</uui-button>
					</umb-template-card>
				</div>`
			)}
			<uui-button id="add-button" look="placeholder" label="open">Add</uui-button>
		`;
	}
}

export default UmbInputTemplatePickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-template-picker': UmbInputTemplatePickerElement;
	}
}
