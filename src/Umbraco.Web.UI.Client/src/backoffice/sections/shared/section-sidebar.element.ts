import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbSectionContext } from '../section.context';
import '../../tree/actions/actions.service';

@customElement('umb-section-sidebar')
export class UmbSectionSidebar extends UmbContextConsumerMixin(LitElement) {
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
	private _sectionName = '';

	@state()
	private _sectionPathname = '';

	private _sectionContext?: UmbSectionContext;
	private _sectionContextSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbSectionContext', (sectionContext: UmbSectionContext) => {
			this._sectionContext = sectionContext;
			this._useSectionContext();
		});
	}

	private _useSectionContext() {
		this._sectionContextSubscription?.unsubscribe();

		this._sectionContextSubscription = this._sectionContext?.data.subscribe((section) => {
			this._sectionName = section.name;
			this._sectionPathname = section.meta.pathname;
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._sectionContextSubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-action-service>
				<uui-scroll-container>
					<a href="${`/section/${this._sectionPathname}`}">
						<h3>${this._sectionName}</h3>
					</a>

					<slot></slot>
				</uui-scroll-container>
			</umb-action-service>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar': UmbSectionSidebar;
	}
}
