import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS } from '../manifests.js';
import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils.js';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

import './insert-section-input.element.js';
// eslint-disable-next-line local-rules/ensure-relative-import-use-js-extension
import type { UmbInsertSectionCheckboxElement } from './insert-section-input.element';

export const UMB_MODAL_TEMPLATING_INSERT_SECTION_MODAL = new UmbModalToken(
	UMB_MODAL_TEMPLATING_INSERT_SECTION_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

export interface InsertSectionModalModalResult {
	value?: string;
}

@customElement('umb-templating-insert-section-modal')
export default class UmbTemplatingInsertSectionModalElement extends UmbModalBaseElement<
	object,
	InsertSectionModalModalResult
> {
	@queryAll('umb-insert-section-checkbox')
	checkboxes!: NodeListOf<UmbInsertSectionCheckboxElement>;

	@state()
	selectedCheckbox?: UmbInsertSectionCheckboxElement | null = null;

	@state()
	snippet = '';

	#chooseSection(event: Event) {
		event.stopPropagation();
		const target = event.target as UmbInsertSectionCheckboxElement;
		const checkboxes = Array.from(this.checkboxes);
		if (checkboxes.every((checkbox) => checkbox.checked === false)) {
			this.selectedCheckbox = null;
			return;
		}
		if (target.checked) {
			this.selectedCheckbox = target;
			this.snippet = this.snippetMethods[checkboxes.indexOf(target)](target?.inputValue as string, target?.isMandatory);
			checkboxes.forEach((checkbox) => {
				if (checkbox !== target) {
					checkbox.checked = false;
				}
			});
		}
	}

	firstUpdated() {
		this.selectedCheckbox = this.checkboxes[0];
	}

	snippetMethods = [getRenderBodySnippet, getRenderSectionSnippet, getAddSectionSnippet];

	#close() {
		this.modalHandler?.reject();
	}

	#submit() {
		if (this.selectedCheckbox?.validate()) this.modalHandler?.submit({ value: this.snippet ?? '' });
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box @change=${this.#chooseSection}>
						<umb-insert-section-checkbox label="Render child template" checked>
							<p slot="description">
								Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.
							</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox label="Render a named section" show-mandatory show-input>
							<p slot="description">
								Renders a named area of a child template, by inserting a <code>@RenderSection(name)</code> placeholder.
								This renders an area of a child template which is wrapped in a corresponding
								<code>@section [name]{ ... }</code> definition.
							</p>
						</umb-insert-section-checkbox>

						<umb-insert-section-checkbox label="Define a named section" show-input>
							<p slot="description">
								Defines a part of your template as a named section by wrapping it in <code>@section { ... }</code>. This
								can be rendered in a specific area of the parent of this template, by using <code>@RenderSection</code>.
							</p>
						</umb-insert-section-checkbox>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary">Close</uui-button>
					<uui-button @click=${this.#submit} look="primary" color="positive">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main umb-insert-section-checkbox:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-section-modal': UmbTemplatingInsertSectionModalElement;
	}
}
