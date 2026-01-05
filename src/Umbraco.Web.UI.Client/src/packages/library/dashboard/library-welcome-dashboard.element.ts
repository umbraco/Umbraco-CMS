import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-library-welcome-dashboard')
export class UmbLibraryWelcomeDashboardElement extends UmbLitElement {
	override render() {
		return html`
			<section id="library-dashboard" class="uui-text">
				<uui-box headline=${this.localize.term('libraryDashboard_welcomeHeader')}>
					<p>
						<umb-localize key="libraryDashboard_welcomeDescription">
							Welcome to the Library section. This is where you can manage your shared content elements.
						</umb-localize>
					</p>
				</uui-box>
			</section>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#library-dashboard {
				display: grid;
				grid-gap: var(--uui-size-7);
				grid-template-columns: 1fr;
				padding: var(--uui-size-layout-1);
			}

			uui-box {
				p:first-child {
					margin-top: 0;
				}
			}
		`,
	];
}

export default UmbLibraryWelcomeDashboardElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-library-welcome-dashboard': UmbLibraryWelcomeDashboardElement;
	}
}
