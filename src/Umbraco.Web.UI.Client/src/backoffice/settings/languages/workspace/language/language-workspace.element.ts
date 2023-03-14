import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbRouteLocation } from '@umbraco-cms/router';
import { UmbWorkspaceEntityElement } from '../../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageModel } from '@umbraco-cms/backend-api';
import UmbLanguageWorkspaceContext from './language-workspace.context';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];

	@property({ type: Object, attribute: false })
	location?: UmbRouteLocation;

	@state()
	_language?: LanguageModel;

	#workspaceContext?: UmbLanguageWorkspaceContext;

	constructor() {
		super();

		this.consumeContext<UmbLanguageWorkspaceContext>('umbWorkspaceContext', (context) => {
			this.#workspaceContext = context;
			this.#observeData();
			this.#init();
		});
	}

	#init() {
		const isoCode = this.location?.params?.isoCode;

		// TODO: implement actions "events" and show loading state
		if (this.location?.name === 'create') {
			this.#workspaceContext?.createScaffold();
		} else if (this.location?.name === 'edit' && isoCode) {
			this.#workspaceContext?.load(isoCode);
		}
	}

	#observeData() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => {
			this._language = data;
		});
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		if (!this._language) return nothing;

		return html`
			<umb-workspace-layout alias="Umb.Workspace.Language">
				<div id="header" slot="header">
					<uui-button href="/section/settings/workspace/language-root" compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input value=${ifDefined(this._language.name)} @input="${this.#handleInput}"></uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
