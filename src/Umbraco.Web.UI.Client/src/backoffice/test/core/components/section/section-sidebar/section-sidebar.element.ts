import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbSectionContext } from '../section.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { ManifestSection } from '@umbraco-cms/models';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import '../../tree/context-menu/tree-context-menu.service';

@customElement('umb-section-sidebar')
export class UmbSectionSidebarElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				flex: 0 0 300px;
				background-color: var(--uui-color-surface);
				height: 100%;
				border-right: 1px solid var(--uui-color-border);
				font-weight: 500;
				display: flex;
				flex-direction: column;
			}

			h3 {
				padding: var(--uui-size-4) var(--uui-size-8);
			}
		`,
	];

	@state()
	private _sectionLabel = '';

	@state()
	private _sectionPathname = '';

	private _sectionContext?: UmbSectionContext;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._observeSectionContext();
		});
	}

	private _observeSectionContext() {
		if (!this._sectionContext) return;

		this.observe<ManifestSection>(this._sectionContext.data, (section) => {
			this._sectionLabel = section.meta.label || section.name;
			this._sectionPathname = section.meta.pathname;
		});
	}

	render() {
		return html`
			<umb-tree-context-menu-service>
				<uui-scroll-container>
					<a href="${`section/${this._sectionPathname}`}">
						<h3>${this._sectionLabel}</h3>
					</a>

					<slot></slot>
				</uui-scroll-container>
			</umb-tree-context-menu-service>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar': UmbSectionSidebarElement;
	}
}
