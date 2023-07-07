import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends UmbLitElement {
	#auth?: typeof UMB_AUTH.TYPE;

	@state()
	private name = '';

	constructor() {
		super();
		this.consumeContext(UMB_AUTH, (instance) => {
			this.#auth = instance;
			this.#observeCurrentUser();
		});
	}

	#observeCurrentUser(): void {
		if (!this.#auth) return;
		this.observe(this.#auth.currentUser, (user) => {
			this.name = user?.name ?? '';
		});
	}

	render() {
		return html`
			<uui-box class="uui-text">
				<h1 class="uui-h2" style="margin-top: var(--uui-size-layout-1);">Welcome, ${this.name}</h1>
				<p class="uui-lead">
					This is a preview version of Umbraco, where you can have a first-hand look at the new Backoffice.
				</p>
				<p>
					There is currently very limited functionality.<br />
					Please refer to the
					<a target="_blank" href="http://docs.umbraco.com/umbraco-backoffice/">documentation</a> to learn more about
					what is possible.
				</p>
			</uui-box>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
