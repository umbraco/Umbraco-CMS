import { html } from 'lit-html';
import { UmbInstallerContext } from '../installer.context';
import '../../core/context/context-provider.element';

export const installerContextProvider = (story: any) => html`
	<umb-context-provider
		style="display: block;margin: 2rem 25%;padding: 1rem;border: 1px solid #ddd;"
		key="umbInstallerContext"
		.value=${new UmbInstallerContext()}>
		${story()}
	</umb-context-provider>
`;
