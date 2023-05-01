import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS } from './manifests';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import {
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalToken,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalHandler,
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UmbDictionaryItemPickerModalResult,
} from '@umbraco-cms/backoffice/modal';

export const UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL = new UmbModalToken(
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS,
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
	#addSection() {
		this.modalHandler?.submit({ value: 'test' });
	}

	#close() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-workspace-layout headline="Insert">
				<div id="main">
					<uui-box>
						<uui-button @click=${this.#addSection} look="placeholder" label="Insert value"
							><h3>Render child template</h3>
							<p>
								Renders the contents of a child template, by inserting a <code>@RenderBody()</code> placeholder.
							</p></uui-button
						>
						<uui-button @click=${this.#addSection} look="placeholder" label="Insert value"
							><h3>Render a named section</h3>
							<p>
								Renders a named area of a child template, by inserting a <code>@RenderSection(name)</code> placeholder.
								This renders an area of a child template which is wrapped in a corresponding
								<code>@section [name]{ ... }</code> definition.
							</p>

							<p>
								<uui-form-layout>
									<uui-label slot="label">Section name</uui-label>
									<uui-input placeholder="Enter section name"></uui-input>
								</uui-form-layout>

								<uui-checkbox>Section is mandatory</uui-checkbox>
								<small
									>If mandatory, the child template must contain a <code>@section</code> definition, otherwise an error
									is shown.</small
								>
							</p></uui-button
						>

						<uui-button @click=${this.#addSection} look="placeholder" label="Insert Macro"
							><h3>Define a named section</h3>
							<p>
								Defines a part of your template as a named section by wrapping it in <code>@section { ... }</code>. This
								can be rendered in a specific area of the parent of this template, by using <code>@RenderSection</code>.
							</p>
							<uui-form-layout>
								<uui-label slot="label">Section name</uui-label>
								<uui-input placeholder="Enter section name"></uui-input>
							</uui-form-layout>
						</uui-button>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary">Close</uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main uui-button:not(:last-of-type) {
				display: block;
				margin-bottom: var(--uui-size-space-5);
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-section-modal': UmbTemplatingInsertSectionModalElement;
	}
}
