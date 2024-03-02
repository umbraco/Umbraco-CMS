import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-settings-welcome-dashboard')
export class UmbSettingsWelcomeDashboardElement extends UmbLitElement {
	render() {
		return html`
			<section id="settings-dashboard" class="uui-text">
				<uui-box>
					<h1 class="uui-h3"><umb-localize key="settingsDashboard_documentationHeader">Documentation</umb-localize></h1>
					<p>
						<umb-localize key="settingsDashboard_documentationDescription">
							Read more about working with the items in Settings in our Documentation.
						</umb-localize>
					</p>
					<uui-button
						look="primary"
						href="https://docs.umbraco.com/umbraco-cms/umbraco-cms"
						label=${this.localize.term('settingsDashboard_getHelp')}
						target="_blank"
						rel="noopener"></uui-button>
				</uui-box>

				<uui-box>
					<h1 class="uui-h3"><umb-localize key="settingsDashboard_communityHeader">Community</umb-localize></h1>
					<p>
						<umb-localize key="settingsDashboard_supportDescription">
							Ask a question in the community forum or our Discord community
						</umb-localize>
					</p>
					<uui-button
						look="primary"
						href="https://our.umbraco.com/forum"
						label=${this.localize.term('settingsDashboard_goForum')}
						target="_blank"
						rel="noopener"></uui-button>
					<uui-button
						look="primary"
						href="https://discord.umbraco.com"
						label=${this.localize.term('settingsDashboard_chatWithCommunity')}
						target="_blank"
						rel="noopener"></uui-button>
				</uui-box>

				<uui-box class="training">
					<h1 class="uui-h3"><umb-localize key="settingsDashboard_trainingHeader">Training</umb-localize></h1>

					<p>
						<umb-localize key="settingsDashboard_trainingDescription">
							Find out about real-life training and certification opportunities
						</umb-localize>
					</p>
					<uui-button
						look="primary"
						href="https://umbraco.com/training/"
						label=${this.localize.term('settingsDashboard_getCertified')}
						target="_blank"
						rel="noopener"></uui-button>
				</uui-box>

				<uui-box>
					<h1 class="uui-h3"><umb-localize key="settingsDashboard_supportHeader">Support</umb-localize></h1>

					<p>
						<umb-localize key="settingsDashboard_supportDescription">
							Ask a question in the community forum or our Discord community.
						</umb-localize>
					</p>
					<uui-button
						look="primary"
						href="https://umbraco.com/support/"
						label=${this.localize.term('settingsDashboard_getHelp')}
						target="_blank"
						rel="noopener"></uui-button>
				</uui-box>

				<uui-box>
					<h1 class="uui-h3"><umb-localize key="settingsDashboard_videosHeader">Videos</umb-localize></h1>
					<p>
						<umb-localize key="settingsDashboard_videosDescription">
							Watch our free tutorial videos on the Umbraco Learning Base YouTube channel, to get upto speed quickly
							with Umbraco.
						</umb-localize>
					</p>
					<uui-button
						look="primary"
						href="https://www.youtube.com/c/UmbracoLearningBase"
						label=${this.localize.term('settingsDashboard_watchVideos')}
						target="_blank"
						rel="noopener"></uui-button>
				</uui-box>
			</section>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#settings-dashboard {
				display: grid;
				grid-gap: var(--uui-size-7);
				grid-template-columns: repeat(3, 1fr);
				padding: var(--uui-size-layout-1);
			}

			@media (max-width: 1200px) {
				#settings-dashboard {
					grid-template-columns: repeat(2, 1fr);
				}
			}

			@media (max-width: 800px) {
				#settings-dashboard {
					grid-template-columns: repeat(1, 1fr);
				}
			}
		`,
	];
}

export default UmbSettingsWelcomeDashboardElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-settings-welcome-dashboard': UmbSettingsWelcomeDashboardElement;
	}
}
