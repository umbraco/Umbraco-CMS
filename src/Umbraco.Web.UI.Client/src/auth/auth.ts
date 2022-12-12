export default function() {
	sessionStorage.setItem('is-authenticated', 'true');
	history.replaceState(null, '', 'section');
}
