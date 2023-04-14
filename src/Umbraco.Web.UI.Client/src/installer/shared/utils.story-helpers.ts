import { html } from 'lit';
import { UmbInstallerContext } from '../installer.context';
import '../../core/context-provider/context-provider.element';

export const installerContextProvider = (story: any, installerContext = new UmbInstallerContext()) => html`
	<umb-context-provider
		style="display: block;margin: 2rem 25%;padding: 1rem;border: 1px solid #ddd;"
		key="umbInstallerContext"
		.value=${installerContext}>
		${story()}
	</umb-context-provider>
`;
