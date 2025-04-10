import type { UmbTemplateCardElement } from '../template-card/template-card.element.js';
import '../template-card/template-card.element.js';
import type { UmbTemplateItemModel } from '../../repository/item/index.js';
import { UmbTemplateItemRepository } from '../../repository/item/index.js';
import { UMB_TEMPLATE_PICKER_MODAL } from '../../modals/index.js';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-template')
export class UmbInputTemplateElement extends UUIFormControlMixin(UmbLitElement, '') {
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
	minMessage = 'This field needs more items';

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

	@property({ type: Array })
	public set selection(newKeys: Array<string> | undefined) {
		this._selection = newKeys ?? [];
		this.#observePickedTemplates();
	}
	public get selection() {
		return this._selection;
	}
	_selection: Array<string> = [];

	_defaultUnique = '';
	@property({ type: String })
	public set defaultUnique(newId: string) {
		this._defaultUnique = newId;
		super.value = newId;
	}
	public get defaultUnique(): string {
		return this._defaultUnique;
	}

	private _templateItemRepository = new UmbTemplateItemRepository(this);

	@state()
	_pickedTemplates: UmbTemplateItemModel[] = [];

	#templatePath = '';

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('template')
			.onSetup(() => {
				return { data: { entityType: 'template', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this.#templatePath = routeBuilder({});
			});
	}

	async #observePickedTemplates() {
		this.observe(
			(await this._templateItemRepository.requestItems(this._selection)).asObservable(),
			(data) => {
				const oldValue = this._pickedTemplates;
				this._pickedTemplates = data;
				this.requestUpdate('_pickedTemplates', oldValue);
			},
			'_observeTemplates',
		);
	}

	protected override getFormElement() {
		return this;
	}

	#appendTemplates(unique: string[]) {
		this.selection = [...(this.selection ?? []), ...unique];

		// If there is no default, set the first picked template as default.
		if (!this.defaultUnique && this.selection.length) {
			this.defaultUnique = this.selection[0];
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	#onCardChange(e: CustomEvent) {
		e.stopPropagation();
		const unique = (e.target as UmbTemplateCardElement).value as string;
		this.defaultUnique = unique;
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #openPicker() {
		const value = await umbOpenModal(this, UMB_TEMPLATE_PICKER_MODAL, {
			data: {
				multiple: true,
				pickableFilter: (template) => template.unique !== null && !this._selection.includes(template.unique),
			},
		}).catch(() => undefined);

		if (!value?.selection) return;

		const selection = value.selection.filter((x) => x !== null) as Array<string>;

		if (!selection.length) return;

		// Add templates to row of picked templates and dispatch change event
		this.#appendTemplates(selection);
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
		this.selection = this._selection.filter((x) => x !== unique);

		// If the default template is removed, set the first picked template as default or reset defaultUnique.
		if (unique === this.defaultUnique) {
			if (this.selection.length) {
				this.defaultUnique = this.selection[0];
			} else {
				this.defaultUnique = '';
			}
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			${this._pickedTemplates.map(
				(template) => html`
					<umb-template-card
						.id=${template.unique}
						.name=${template.name}
						@change=${this.#onCardChange}
						@open=${() => window.history.pushState({}, '', this.#templatePath + 'edit/' + template.unique)}
						?default=${template.unique === this.defaultUnique}>
						<uui-button
							slot="actions"
							compact
							label=${this.localize.term('general_remove') + ' ' + template.name}
							@click=${() => this.#removeTemplate(template.unique ?? '')}>
							<uui-icon name="icon-trash"></uui-icon>
						</uui-button>
					</umb-template-card>
				`,
			)}
			<uui-button
				id="btn-add"
				look="placeholder"
				label=${this.localize.term('general_choose')}
				@click=${this.#openPicker}></uui-button>
		`;
	}

	static override styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
				grid-template-rows: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
			}

			#btn-add {
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
