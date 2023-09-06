import { UUITextStyles } from "@umbraco-ui/uui-css";
import { css } from "lit";


export const UmbTextStyles = css`
	${UUITextStyles}

  a {
		color: var(--uui-color-interactive);
	}
	a:hover,
	a:focus {
		color: var(--uui-color-interactive-emphasis);
	}
`
