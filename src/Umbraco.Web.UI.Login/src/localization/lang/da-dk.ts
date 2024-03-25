import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
  auth: {
    continue: 'Fortsæt',
    validate: 'Indsend',
    login: 'Log ind',
    email: 'E-mail',
    username: 'Brugernavn',
    password: 'Adgangskode',
    submit: 'Indsend',
    required: 'Påkrævet',
    success: 'Succes',
    forgottenPassword: 'Glemt adgangskode?',
    forgottenPasswordInstruction: 'En e-mail vil blive sendt til den angivne adresse med et link til at nulstille din adgangskode',
    requestPasswordResetConfirmation: 'En e-mail med instruktioner for nulstilling af adgangskoden vil blive sendt til den angivne adresse, hvis det matcher vores optegnelser',
    setPasswordConfirmation: 'Din adgangskode er blevet opdateret',
    rememberMe: 'Husk mig',
    or: 'eller',
    error: 'Fejl',
    defaultError: 'Der er opstået en ukendt fejl.',
    errorInPasswordFormat: 'Kodeordet skal være på minimum %0% tegn og indeholde mindst %1% alfanumeriske tegn.',
    passwordMismatch: 'Adgangskoderne er ikke ens.',
    passwordMinLength: 'Adgangskoden skal være mindst {0} tegn lang.',
    passwordIsBlank: 'Din nye adgangskode kan ikke være tom.',
    userFailedLogin: 'Ups! Vi kunne ikke logge dig ind. Tjek at dit brugernavn og adgangskode er korrekt og prøv igen.',
    receivedErrorFromServer: 'Der skete en fejl på serveren',
    resetCodeExpired: 'Det link, du har klikket på, er ugyldigt eller udløbet',
    userInviteWelcomeMessage: 'Hej og velkommen til Umbraco! På bare 1 minut vil du være klar til at komme i gang, vi skal bare have dig til at oprette en adgangskode.',
    userInviteExpiredMessage: 'Velkommen til Umbraco! Desværre er din invitation udløbet. Kontakt din administrator og bed om at gensende invitationen.',
    newPassword: 'Ny adgangskode',
    confirmNewPassword: 'Bekræft adgangskode',
    greeting0: 'Velkommen',
    greeting1: 'Velkommen',
    greeting2: 'Velkommen',
    greeting3: 'Velkommen',
    greeting4: 'Velkommen',
    greeting5: 'Velkommen',
    greeting6: 'Velkommen',
    mfaTitle: 'Sidste skridt!',
    mfaCodeInputHelp: 'Indtast venligst bekræftelseskoden',
    mfaText: 'Du har aktiveret multi-faktor godkendelse. Du skal nu bekræfte din identitet.',
    mfaMultipleText: 'Vælg venligst en godkendelsesmetode',
    mfaCodeInput: 'Kode',
    signInWith: 'Log ind med {0}',
    returnToLogin: 'Tilbage til log ind',
  },
} satisfies UmbLocalizationDictionary;