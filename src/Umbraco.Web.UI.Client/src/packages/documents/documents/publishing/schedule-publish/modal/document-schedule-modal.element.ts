import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../../types.js';
import { UmbDocumentVariantLanguagePickerElement } from '../../../modals/index.js';
import type { UmbDocumentScheduleModalData, UmbDocumentScheduleModalValue } from './document-schedule-modal.token.js';
import { css, customElement, html, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import type { UUIBooleanInputElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-schedule-modal')
export class UmbDocumentScheduleModalElement extends UmbModalBaseElement<
	UmbDocumentScheduleModalData,
	UmbDocumentScheduleModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	@state()
	_selection: UmbDocumentScheduleModalValue['selection'] = [];

	@state()
	_isAllSelected?: boolean;

	constructor() {
		super();
		this.observe(
			this.#selectionManager.selection,
			(selection) => {
				this._selection = selection.map((unique) => {
					return { unique, schedule: {} };
				});
				this._isAllSelected = this.#isAllSelected();
			},
			'_selection',
		);
	}

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		const pickableFilter = this.data?.pickableFilter;

		if (pickableFilter) {
			this.#selectionManager.setAllowLimitation((unique) => {
				const option = this.data?.options.find((o) => o.unique === unique);
				return option ? pickableFilter(option) : true;
			});
		}

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		// TODO:[NL] I would say we should change this, the act of scheduling should be equivalent to save & publishing. Resulting in content begin saved as part of carrying out the action. (But this requires a update in the workspace.)
		this._options =
			this.data?.options.filter(
				(option) => option.variant && option.variant.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this._options.some((o) => o.unique === s.unique));

		this.#selectionManager.setSelection(selected.map((s) => s.unique));
	}

	#submit() {
		this.value = { selection: this._selection };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	#isSelected(unique: string) {
		return this._selection.some((s) => s.unique === unique);
	}

	#onSelectAllChange(event: Event) {
		const allUniques = this._options.map((o) => o.unique);
		const filter = this.#selectionManager.getAllowLimitation();
		const allowedUniques = allUniques.filter((unique) => filter(unique));

		if ((event.target as UUIBooleanInputElement).checked) {
			this.#selectionManager.setSelection(allowedUniques);
		} else {
			this.#selectionManager.setSelection([]);
		}
	}

	#isAllSelected() {
		const allUniques = this._options.map((o) => o.unique);
		const filter = this.#selectionManager.getAllowLimitation();
		const allowedUniques = allUniques.filter((unique) => filter(unique));
		return this._selection.length === allowedUniques.length;
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('general_scheduledPublishing')}>
			<p id="subtitle">
				${when(
					this._options.length > 1,
					() => html`
						<umb-localize key="content_languagesToSchedule">Which languages would you like to schedule?</umb-localize>
					`,
					() => html`
						<umb-localize key="content_schedulePublishHelp">
							Select the date and time to publish and/or unpublish the content item.
						</umb-localize>
					`,
				)}
			</p>

			${this.#renderOptions()}

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('buttons_schedulePublish')}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	#renderOptions() {
		return html`
			<uui-checkbox
				@change=${this.#onSelectAllChange}
				label=${this.localize.term('general_selectAll')}
				.checked=${this._isAllSelected ?? false}></uui-checkbox>

			${repeat(
				this._options,
				(option) => option.unique,
				(option) => this.#renderItem(option),
			)}
		`;
	}

	#renderItem(option: UmbDocumentVariantOptionModel) {
		const pickable = this.data?.pickableFilter ? this.data.pickableFilter(option) : () => true;

		return html`
			<uui-menu-item
				?selectable=${pickable}
				?disabled=${!pickable}
				label=${option.variant?.name ?? option.language.name}
				@selected=${() => this.#selectionManager.select(option.unique)}
				@deselected=${() => this.#selectionManager.deselect(option.unique)}
				?selected=${this.#isSelected(option.unique)}>
				<uui-icon slot="icon" name="icon-globe"></uui-icon>
				${UmbDocumentVariantLanguagePickerElement.renderLabel(option)}
			</uui-menu-item>
			${when(this.#isSelected(option.unique), () => this.#renderPublishDateInput(option))}
		`;
	}

	#renderPublishDateInput(option: UmbDocumentVariantOptionModel) {
		return html`<div class="publish-date">
			<uui-form-layout-item>
				<uui-label slot="label"><umb-localize key="content_releaseDate">Publish at</umb-localize></uui-label>
				<div>
					<umb-input-date
						type="datetime-local"
						@change=${(e: Event) => this.#onFromDateChange(e, option.unique)}
						label=${this.localize.term('general_publishDate')}></umb-input-date>
				</div>
			</uui-form-layout-item>
			<uui-form-layout-item>
				<uui-label slot="label"><umb-localize key="content_unpublishDate">Unpublish at</umb-localize></uui-label>
				<div>
					<umb-input-date
						type="datetime-local"
						@change=${(e: Event) => this.#onToDateChange(e, option.unique)}
						label=${this.localize.term('general_publishDate')}></umb-input-date>
				</div>
			</uui-form-layout-item>
		</div>`;
	}

	#onFromDateChange(e: Event, unique: string) {
		const variant = this._selection.find((s) => s.unique === unique);
		if (variant) {
			variant.schedule = {
				...variant.schedule,
				publishTime: (e.target as UmbInputDateElement).value.toString(),
			};
		}
	}

	#onToDateChange(e: Event, unique: string) {
		const variant = this._selection.find((s) => s.unique === unique);
		if (variant) {
			variant.schedule = {
				...variant.schedule,
				unpublishTime: (e.target as UmbInputDateElement).value.toString(),
			};
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				min-width: 600px;
				max-width: 90vw;
			}

			.label {
				padding: 0.5rem 0;
			}
			.label-status {
				font-size: 0.8rem;
			}

			.publish-date {
				display: flex;
				flex-direction: row;
				justify-content: space-between;
				gap: 1rem;
				border-top: 1px solid var(--uui-color-border);
				border-bottom: 1px solid var(--uui-color-border);
			}

			.publish-date > uui-form-layout-item {
				flex: 1;
				margin: 0;
				padding: 0.5rem 0 1rem;
			}

			.publish-date > uui-form-layout-item:first-child {
				border-right: 1px dashed var(--uui-color-border);
			}

			uui-checkbox {
				margin-bottom: var(--uui-size-space-3);
			}

			uui-menu-item {
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

export default UmbDocumentScheduleModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-schedule-modal': UmbDocumentScheduleModalElement;
	}
}
