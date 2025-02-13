import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../../types.js';
import { isNotPublishedMandatory } from '../../utils.js';
import { UmbDocumentVariantLanguagePickerElement } from '../../../modals/index.js';
import type {
	UmbDocumentScheduleModalData,
	UmbDocumentScheduleModalValue,
	UmbDocumentScheduleSelectionModel,
} from './document-schedule-modal.token.js';
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
	_hasNotSelectedMandatory?: boolean;

	@state()
	_selection: UmbDocumentScheduleModalValue['selection'] = [];

	@state()
	_isAllSelected = false;

	#pickableFilter = (option: UmbDocumentVariantOptionModel) => {
		if (!option.variant || option.variant.state === UmbDocumentVariantState.NOT_CREATED) {
			// If not data present, then its not pickable.
			return false;
		}
		return this.data?.pickableFilter ? this.data.pickableFilter(option) : true;
	};

	constructor() {
		super();
		this.observe(
			this.#selectionManager.selection,
			(selection) => {
				if (!this._options && !selection) return;

				// New selections are mapped to the schedule data
				this._selection = selection.map<UmbDocumentScheduleSelectionModel>((unique) => {
					return { unique, schedule: this.#getSchedule(unique) };
				});
				this._isAllSelected = this.#isAllSelected();

				//Getting not published mandatory options — the options that are mandatory and not currently published.
				const missingMandatoryOptions = this._options.filter(isNotPublishedMandatory);
				this._hasNotSelectedMandatory = missingMandatoryOptions.some((option) => !selection.includes(option.unique));
			},
			'_selection',
		);
	}

	#getSchedule(unique: string): UmbDocumentScheduleSelectionModel['schedule'] {
		const matchedItem = this.value?.selection.find((s) => s.unique === unique);
		return matchedItem?.schedule ?? {};
	}

	override firstUpdated() {
		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		this.#selectionManager.setAllowLimitation((unique) => {
			const option = this._options.find((o) => o.unique === unique);
			return option ? this.#pickableFilter(option) : true;
		});

		// Only display variants that are relevant to pick from, i.e. variants that are draft, not-published-mandatory or published with pending changes.
		// If we don't know the state (e.g. from a bulk publishing selection) we need to consider it available for selection.
		this._options =
			this.data?.options.filter(
				(option) =>
					(option.variant && option.variant.state === null) ||
					isNotPublishedMandatory(option) ||
					option.variant?.state !== UmbDocumentVariantState.NOT_CREATED,
			) ?? [];

		let selected = this.data?.activeVariants ?? [];

		const validOptions = this._options.filter((o) => this.#pickableFilter!(o));

		// Filter selection based on options:
		selected = selected.filter((unique) => validOptions.some((o) => o.unique === unique));

		this.#selectionManager.setSelection(selected);
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
		return this._selection.length !== 0 && this._selection.length === allowedUniques.length;
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
					?disabled=${!this._selection.length || this._hasNotSelectedMandatory}
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	#renderOptions() {
		return html`
			${when(
				this._options.length > 1,
				() => html`
					<uui-checkbox
						@change=${this.#onSelectAllChange}
						label=${this.localize.term('general_selectAll')}
						.checked=${this._isAllSelected}></uui-checkbox>
				`,
			)}
			${repeat(
				this._options,
				(option) => option.unique,
				(option) => this.#renderItem(option),
			)}
		`;
	}

	#renderItem(option: UmbDocumentVariantOptionModel) {
		const pickable = this.#pickableFilter(option);

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
						.value=${this.#formatDate(option.variant?.scheduledPublishDate)}
						@change=${(e: Event) => this.#onFromDateChange(e, option.unique)}
						label=${this.localize.term('general_publishDate')}></umb-input-date>
				</div>
			</uui-form-layout-item>
			<uui-form-layout-item>
				<uui-label slot="label"><umb-localize key="content_unpublishDate">Unpublish at</umb-localize></uui-label>
				<div>
					<umb-input-date
						type="datetime-local"
						.value=${this.#formatDate(option.variant?.scheduledUnpublishDate)}
						@change=${(e: Event) => this.#onToDateChange(e, option.unique)}
						label=${this.localize.term('general_publishDate')}></umb-input-date>
				</div>
			</uui-form-layout-item>
		</div>`;
	}

	/**
	 * Formats the date to be compatible with the input type datetime-local
	 * @param {string} dateStr The date to format, example: 2021-01-01T12:00:00.000+01:00
	 * @returns {string | undefined} The formatted date in local time with no offset, example: 2021-01-01T11:00
	 */
	#formatDate(dateStr?: string | null): string | undefined {
		if (!dateStr) return undefined;

		const d = new Date(dateStr);

		if (isNaN(d.getTime())) {
			console.warn('[Schedule]: Invalid date:', dateStr);
			return undefined;
		}

		// We need to subtract the offset to get the correct time in the input field
		// the input field expects local time without offset and the Date object will convert the date to local time
		return (
			d.getFullYear() +
			'-' +
			String(d.getMonth() + 1).padStart(2, '0') +
			'-' +
			String(d.getDate()).padStart(2, '0') +
			'T' +
			String(d.getHours()).padStart(2, '0') +
			':' +
			String(d.getMinutes()).padStart(2, '0')
		);
	}

	#onFromDateChange(e: Event, unique: string) {
		const variant = this._selection.find((s) => s.unique === unique);
		if (!variant) return;
		variant.schedule = {
			...variant.schedule,
			publishTime: this.#getDateValue(e),
		};
	}

	#onToDateChange(e: Event, unique: string) {
		const variant = this._selection.find((s) => s.unique === unique);
		if (!variant) return;
		variant.schedule = {
			...variant.schedule,
			unpublishTime: this.#getDateValue(e),
		};
	}

	#getDateValue(e: Event): string | null {
		const value = (e.target as UmbInputDateElement).value.toString();
		return value.length ? value : null;
	}

	static override readonly styles = [
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
				margin-top: var(--uui-size-space-4);
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
