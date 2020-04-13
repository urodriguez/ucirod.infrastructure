﻿using System;
using System.Collections.Generic;
using Logging.Application.Dtos;

namespace Logging.Application
{
    public interface ILogService
    {
        //From Controllers
        void Log(LogDtoPost logDto);
        IEnumerable<LogSearchResponseDto> Search(LogSearchRequestDto logSearchRequestDto);

        //Internals
        Guid GetInternalCorrelationId();
        void InternalLogTraceMessage(string messageToLog);
        void InternalLogInfoMessage(string messageToLog);
        void InternalLogErrorMessage(string messageToLog);
        string InternalFileSystemLog(string messageToLog);
    }
}